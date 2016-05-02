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
        public HttpResponseMachine(ParserStore parserStore = null) 
            : base(null,parserStore)
        {
        }
    }

    public class HttpResponseMachine<T> : IResponseHandler
    {
        private readonly T _Model;
        private readonly List<ActionRegistration> _ResponseActions = new List<ActionRegistration>();
        private readonly ParserStore _ParserStore;
        
        public delegate Task ResponseAction<T>(T clientstate, string linkRelation, HttpResponseMessage response);

        public HttpResponseMachine(T model, ParserStore parserStore = null)
        {
            if (parserStore == null)
            {
                parserStore = new ParserStore();
            }

            _ParserStore = parserStore;
            _Model = model;
        }

        public async Task<HttpResponseMessage> HandleResponseAsync(string linkrelation, HttpResponseMessage response)
        {
            var actionKey = ActionKey.CreateActionKey(response, linkrelation);

            var selectedAction = FindBestMatchAction(actionKey);

            await selectedAction.ResponseAction(_Model, linkrelation, response);

            return response;
        }

        public IActionRegistrationBuilder When(HttpStatusCode statusCode, string linkRelation = null, MediaTypeHeaderValue contentType = null, Uri profile = null)
        {
            var key = new ActionKey()
            {
                StatusCode = statusCode,
                ContentType = contentType,
                Profile = profile,
                LinkRelation = linkRelation,
            };
            return new ActionRegistrationBuilder(key, this);
        }

        private SearchResult FindBestMatchAction(ActionKey actionKey)
        {
            // Filter Actions to only include those that match the status code
            var actionsByStatus = _ResponseActions.Where(a => a.Key.StatusCode == actionKey.StatusCode);

            // If there aren't any, then match with the default status code X00 for that class
            if (!actionsByStatus.Any())
            {
                actionKey.StatusCode = GetDefaultStatusCode(actionKey.StatusCode);
            }

            var candidateActions = actionsByStatus.Where(h => h.Key.Match(actionKey))
                .Select(h => new SearchResult()
                {
                    ResponseAction = h.ResponseAction,
                    Score = h.Key.Score()
                }).ToList();

            if (!candidateActions.Any()) throw new Exception(String.Format("No handler configured for response: Status Code {0}, Media Type {1}, Link Relation {2}", actionKey.StatusCode, actionKey.ContentType, actionKey.LinkRelation));

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

        public interface IActionRegistrationBuilder
        {
            void Then(ResponseAction<T> action);
            void Then(Func<string, HttpResponseMessage, Task> action);
            void Then<TARGET>(Action<T, string, TARGET> action);

        }

        private class ActionRegistrationBuilder : IActionRegistrationBuilder  
        {
            private List<ActionRegistration> _Registrations = new List<ActionRegistration>();
            private HttpResponseMachine<T> _Machine;
            private ActionKey _ActionKey;

            public ActionRegistrationBuilder(ActionKey actionKey, HttpResponseMachine<T> machine)
            {
                _ActionKey = actionKey;
                _Machine = machine;
            }

            public void Then(ResponseAction<T> action)
            {
                var reg = new ActionRegistration(_ActionKey,action);
                _Machine._ResponseActions.Add(reg);

            }

            public void Then(Func<string, HttpResponseMessage, Task> action)
            {
                    var reg = new ActionRegistration(_ActionKey, (m, l, r) => action(l, r));
                    _Machine._ResponseActions.Add(reg);
            }

            public void Then<Target>(Action<T, string, Target> responseAction)
            {
                var regs = new List<ActionRegistration>();
                // Look for media type translator for this target type
                var mediaTypeParsers = _Machine._ParserStore.GetMediaTypeParsers<Target>();

                if (mediaTypeParsers.Any())
                {
                    foreach (var parserInfo in mediaTypeParsers)
                    {
                        var p = parserInfo;
                        ResponseAction<T> action = async (m, l, r) =>
                        {
                            var target = await p.Parse(r.Content);
                            responseAction(m, l, (Target)target);
                        };
                        var registration = new ActionRegistration(_ActionKey, action);
                        _Machine._ResponseActions.Add(registration);

                    }
                }
                else
                {
                    // Find AppSemantic parsers for the target type
                    var semanticParsers = _Machine._ParserStore.GetSemanticParsers<Target>(_ActionKey.LinkRelation);

                    foreach (var parserInfo in semanticParsers)
                    {
                        var key = new ActionKey()
                        {
                            StatusCode = _ActionKey.StatusCode,
                            ContentType = new MediaTypeHeaderValue(parserInfo.MediaTypeParser.MediaType),
                            Profile = parserInfo.ProfileParser.Profile,
                            LinkRelation = parserInfo.ProfileParser.LinkRelation,
                        };
                        ResponseAction<T> action = async (m, l, r) =>
                        {
                            var mt = await parserInfo.MediaTypeParser.Parse(r.Content);
                            var target = (Target)parserInfo.ProfileParser.Parse(mt);
                            responseAction(m, l, target);
                        };

                        var registration = new ActionRegistration(key, action);
                        _Machine._ResponseActions.Add(registration);
                    }

                }


            }
        }

        private class ActionRegistration
        {
      
            public ActionRegistration(ActionKey key, ResponseAction<T> responseAction)
            {
                Key = key;
                ResponseAction = responseAction;
            }

            public ActionKey Key { get; private set; }
            public ResponseAction<T> ResponseAction { get; private set; }
        }

        private class ActionKey
        {
            public int Score()
            {
                return (ContentType != null ? 8 : 0) + (LinkRelation != null ? 2 : 0) + (Profile != null ? 2 : 0);
            }

            public bool Match(ActionKey test)
            {
                return StatusCode == test.StatusCode
                        && (ContentType == null || ContentType.MediaType.Equals(test.ContentType.MediaType))
                        && (Profile == null || Profile == test.Profile)
                        && (String.IsNullOrEmpty(LinkRelation) || LinkRelation == test.LinkRelation);
            }

            public static ActionKey CreateActionKey(HttpResponseMessage response, string linkRelation)
            {
                var key = new ActionKey();
                key.StatusCode = response.StatusCode;
                if (response.Content != null)
                {
                    key.ContentType = response.Content.Headers.ContentType;
                    // Hunt for profile (m/t Parameters, Link Header)
                    if (key.ContentType != null)
                    {
                        var profile = key.ContentType.Parameters.FirstOrDefault(p => p.Name == "profile");
                        if (profile != null)
                        {
                            key.Profile = new Uri(profile.Value.Substring(1, profile.Value.Length - 2));
                        }
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

        private class SearchResult
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
