using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Hapikit.Links;

namespace Hapikit.ResponseHandlers
{


    public class HttpResponseMachine : HttpResponseMachine<object>
    {
        public HttpResponseMachine()
            : base(null)
        {
        }

        public void AddResponseAction(Func<string, HttpResponseMessage,Task> responseHandler, HttpStatusCode statusCode, string linkRelation = null, MediaTypeHeaderValue contentType = null, Uri profile = null)
        {
            this.AddResponseAction((m, l, r) => responseHandler(l, r), statusCode, linkRelation: linkRelation, contentType: contentType, profile: profile);
        }

        public void AddResponseAction<Target>(Action<string, Target> responseAction, HttpStatusCode statusCode, string linkRelation = null)
        {
            // Look for media type translator for this target type
            var mediaTypeParsers = _MediaTypeParsers.Values.Where(pi => pi.TargetType == typeof(Target)).ToList();

            if (!mediaTypeParsers.Any())
            {
                // Find Profile parsers for the target type
                var doubleParsers = _ProfileParsers.Values
                    .Where(pi => pi.TargetType == typeof(Target))
                    .SelectMany(
                                    pp=> _MediaTypeParsers.Values.Where(pi => pi.TargetType == pp.SourceType)
                                        .Select(mt => new { MediaTypeParser = mt, ProfileParser = pp }));

                foreach (var parserInfo in doubleParsers)
                {
                    this.AddResponseAction(async (m, l, r) =>
                    {
                        var mt = parserInfo.MediaTypeParser.Parse(r.Content);
                        var target = (Target)parserInfo.ProfileParser.Parse(mt);
                        responseAction(l, target);

                    }, statusCode, linkRelation: linkRelation, contentType: new MediaTypeHeaderValue(parserInfo.MediaTypeParser.MediaType),profile: parserInfo.ProfileParser.Profile);

                }
               
            }
    
            foreach(var parserInfo in mediaTypeParsers)
            {
                var p = parserInfo;
                this.AddResponseAction(async (m, l, r) =>
                {
                    var target = await p.Parse(r.Content);
                    responseAction(l, (Target)target);

                }, statusCode, linkRelation: linkRelation, contentType: new MediaTypeHeaderValue(parserInfo.MediaType));

            }

        }
    }

    public class HttpResponseMachine<T> : IResponseHandler
    {
        private readonly T _Model;
        private readonly List<ActionKey> _ResponseActions = new List<ActionKey>();
        protected Dictionary<string, ParserInfo> _MediaTypeParsers = new Dictionary<string, ParserInfo>();
        protected Dictionary<Uri, ProfileParserInfo> _ProfileParsers = new Dictionary<Uri, ProfileParserInfo>();
        
        public delegate Task ResponseAction<T>(T clientstate, string linkRelation, HttpResponseMessage response);

        public HttpResponseMachine(T model)
        {
            _Model = model;
        }

        public async Task<HttpResponseMessage> HandleResponseAsync(string linkrelation, HttpResponseMessage response)
        {
            var actionKey = new ActionKey(response, linkrelation);

            var selectedAction = FindBestMatchAction(actionKey);

            await selectedAction.ResponseAction(_Model, linkrelation, response);

            return response;
        }

        private HandlerResult FindBestMatchAction(ActionKey responseActionKey)
        {
            var statusHandlers = _ResponseActions.Where(h => h.StatusCode == responseActionKey.StatusCode);
            if (!statusHandlers.Any())
            {
                responseActionKey.StatusCode = GetDefaultStatusCode(responseActionKey.StatusCode);
            }


            var handlerResults = statusHandlers.Where(h => h.StatusCode == responseActionKey.StatusCode
                                                           && (h.ContentType == null ||
                                                            h.ContentType.Equals(responseActionKey.ContentType))
                                                           && (h.Profile == null || h.Profile == responseActionKey.Profile)
                                                           && (String.IsNullOrEmpty(h.LinkRelation) ||
                                                            h.LinkRelation == responseActionKey.LinkRelation))
                .Select(h => new HandlerResult()
                {
                    ResponseAction = h.ResponseAction,
                    Score = (h.ContentType != null ? 8 : 0) + (h.LinkRelation != null ? 2 : 0) + (h.Profile != null ? 2 : 0)
                });

            if (!handlerResults.Any()) throw new Exception(String.Format("No handler configured for response: Status Code {0}, Media Type {1}, Link Relation {2}", responseActionKey.StatusCode, responseActionKey.ContentType, responseActionKey.LinkRelation));
            var handler = handlerResults.OrderByDescending(h => h.Score).First();
            return handler;
        }
        private HttpStatusCode GetDefaultStatusCode(HttpStatusCode httpStatusCode)
        {
            if ((int)httpStatusCode < 200)
            {
                return HttpStatusCode.Continue; // 100
            }
            else if ((int)httpStatusCode < 300)
            {
                return HttpStatusCode.OK; //200
            }
            else if ((int)httpStatusCode < 400)
            {
                return HttpStatusCode.MultipleChoices; // 300
            }
            else if ((int)httpStatusCode < 500)
            {
                return HttpStatusCode.BadRequest; // 400
            }
            else
            {
                return HttpStatusCode.InternalServerError; // 500
            }
        }


       public void AddResponseAction(ResponseAction<T> responseAction, HttpStatusCode statusCode, string linkRelation = null, MediaTypeHeaderValue contentType = null, Uri profile = null)
        {
            var key = new ActionKey()
            {
                StatusCode = statusCode,
                ContentType = contentType,
                Profile = profile,
                LinkRelation = linkRelation,
                ResponseAction = responseAction
            };
            _ResponseActions.Add(key);
        }

       public void AddMediaTypeParser<Target>(string mediaType, Func<HttpContent, Task<Target>> translator) where Target:class
        {
            _MediaTypeParsers[mediaType] = new ParserInfo
            {
                MediaType = mediaType,
                TargetType = typeof(Target),
                Parse = async (c) => await translator(c)
            }; 
        }

       public void AddProfileParser<Source,Target>(Uri profile, Func<Source, Target> translator) where Target : class
       {
           _ProfileParsers[profile] = new ProfileParserInfo
           {
               Profile = profile,
               SourceType = typeof(Source),
               TargetType = typeof(Target),
               Parse = (s) => translator((Source)s)
           };
       }

    
        protected struct ParserInfo
        {
            public string MediaType;
            public string Profile;
            public Type TargetType;
            public Func<HttpContent, Task<object>> Parse;
        }
        
        protected struct ProfileParserInfo
        {
            public Uri Profile;
            public Type SourceType;
            public Type TargetType;
            public Func<object, object> Parse;
        }

        private class ActionKey
        {
            public ActionKey()
            {

            }

            public ActionKey(HttpResponseMessage response, string linkRelation)
            {
                StatusCode = response.StatusCode;
                if (response.Content != null)
                {
                    ContentType = response.Content.Headers.ContentType;
                    // Hunt for profile (m/t Parameters, Link Header)
                }
                LinkRelation = linkRelation;
            }
            public HttpStatusCode StatusCode { get; set; }
            public MediaTypeHeaderValue ContentType { get; set; }
            public Uri Profile { get; set; }
            public string LinkRelation { get; set; }
            public ResponseAction<T> ResponseAction { get; set; }

        }

        private class HandlerResult
        {
            public int Score { get; set; }
            public ResponseAction<T> ResponseAction { get; set; }
        }


    }

    public class Model<T>
    {
        public T Value { get; set; }
    }

}
