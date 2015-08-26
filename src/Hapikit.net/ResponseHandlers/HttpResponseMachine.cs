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
                // Find AppSemantic parsers for the target type
                var doubleParsers = _AppSemanticParsers.Values
                    .Where(pi => pi.TargetType == typeof(Target) && (linkRelation == null || linkRelation == pi.LinkRelation))
                    .SelectMany(
                                    pp=> _MediaTypeParsers.Values.Where(pi => pi.TargetType == pp.SourceType)
                                        .Select(mt => new { MediaTypeParser = mt, ProfileParser = pp }));

                foreach (var parserInfo in doubleParsers)
                {
                    this.AddResponseAction(async (m, l, r) =>
                    {
                        var mt = await parserInfo.MediaTypeParser.Parse(r.Content);
                        var target = (Target)parserInfo.ProfileParser.Parse(mt);
                        responseAction(l, target);

                    }, statusCode, linkRelation: parserInfo.ProfileParser.LinkRelation, contentType: new MediaTypeHeaderValue(parserInfo.MediaTypeParser.MediaType),profile: parserInfo.ProfileParser.Profile);

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
        private readonly List<ActionRegistration> _ResponseActions = new List<ActionRegistration>();
        protected Dictionary<string, ParserInfo> _MediaTypeParsers = new Dictionary<string, ParserInfo>();
        protected Dictionary<string, AppSemanticsParserInfo> _AppSemanticParsers = new Dictionary<string, AppSemanticsParserInfo>();

        
        public delegate Task ResponseAction<T>(T clientstate, string linkRelation, HttpResponseMessage response);

        public HttpResponseMachine(T model)
        {
            _Model = model;
        }

        public async Task<HttpResponseMessage> HandleResponseAsync(string linkrelation, HttpResponseMessage response)
        {
            var actionKey = new ActionRegistration(response, linkrelation);

            var selectedAction = FindBestMatchAction(actionKey);

            await selectedAction.ResponseAction(_Model, linkrelation, response);

            return response;
        }

        private HandlerResult FindBestMatchAction(ActionRegistration responseActionRegistration)
        {
            // Filter Actions to only include those that match the status code
            var actionsByStatus = _ResponseActions.Where(h => h.StatusCode == responseActionRegistration.StatusCode);

            // If there aren't any, then match with the default status code X00 for that class
            if (!actionsByStatus.Any())
            {
                responseActionRegistration.StatusCode = GetDefaultStatusCode(responseActionRegistration.StatusCode);
            }


            var candidateActions = actionsByStatus.Where(h => h.StatusCode == responseActionRegistration.StatusCode
                                                           && (h.ContentType == null ||
                                                            h.ContentType.MediaType.Equals(responseActionRegistration.ContentType.MediaType))
                                                           && (h.Profile == null || h.Profile == responseActionRegistration.Profile)
                                                           && (String.IsNullOrEmpty(h.LinkRelation) ||
                                                            h.LinkRelation == responseActionRegistration.LinkRelation))
                .Select(h => new HandlerResult()
                {
                    ResponseAction = h.ResponseAction,
                    Score = (h.ContentType != null ? 8 : 0) + (h.LinkRelation != null ? 2 : 0) + (h.Profile != null ? 2 : 0)
                }).ToList();

            if (!candidateActions.Any()) throw new Exception(String.Format("No handler configured for response: Status Code {0}, Media Type {1}, Link Relation {2}", responseActionRegistration.StatusCode, responseActionRegistration.ContentType, responseActionRegistration.LinkRelation));

            // Select the best match based on Score
            var selectedAction = candidateActions.OrderByDescending(h => h.Score).First();
            return selectedAction;
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
            var key = new ActionRegistration()
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
           _AppSemanticParsers[profile.AbsoluteUri] = new AppSemanticsParserInfo
           {
               Profile = profile,
               SourceType = typeof(Source),
               TargetType = typeof(Target),
               Parse = (s) => translator((Source)s)
           };
       }

       public void AddLinkRelationParser<Source, Target>(string linkrelation, Func<Source, Target> translator) where Target : class
       {
           _AppSemanticParsers[linkrelation] = new AppSemanticsParserInfo
           {
               LinkRelation = linkrelation,
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
        
        protected struct AppSemanticsParserInfo
        {
            public Uri Profile;
            public string LinkRelation;
            public Type SourceType;
            public Type TargetType;
            public Func<object, object> Parse;
        }

        private class ActionRegistration
        {
      
            public ActionRegistration()
            {

            }

            public ActionRegistration(HttpResponseMessage response, string linkRelation)
            {
                StatusCode = response.StatusCode;
                if (response.Content != null)
                {
                    ContentType = response.Content.Headers.ContentType;
                    // Hunt for profile (m/t Parameters, Link Header)
                    
                    var profile = ContentType.Parameters.FirstOrDefault(p => p.Name == "profile");
                    if (profile != null)
                    {
                        Profile = new Uri(profile.Value.Substring(1,profile.Value.Length -2));
                    }
                }
                LinkRelation = linkRelation;
            }
            public HttpStatusCode StatusCode { get; set; }
            public MediaTypeHeaderValue ContentType { get; set; }
            public Uri Profile { get; set; }
            public string LinkRelation { get; set; }
            public ResponseAction<T> ResponseAction { get; set; }

        }

        private class ActionKey
        {

            public static ActionKey CreateActionKey(HttpResponseMessage response, string linkRelation)
            {
                var key = new ActionKey();
                key.StatusCode = response.StatusCode;
                if (response.Content != null)
                {
                    key.ContentType = response.Content.Headers.ContentType;
                    // Hunt for profile (m/t Parameters, Link Header)

                    var profile = key.ContentType.Parameters.FirstOrDefault(p => p.Name == "profile");
                    if (profile != null)
                    {
                        key.Profile = new Uri(profile.Value.Substring(1, profile.Value.Length - 2));
                    }
                }
                key.LinkRelation = linkRelation;
                return key;
            }
            public HttpStatusCode StatusCode { get; set; }
            public MediaTypeHeaderValue ContentType { get; set; }
            public Uri Profile { get; set; }
            public string LinkRelation { get; set; }
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
