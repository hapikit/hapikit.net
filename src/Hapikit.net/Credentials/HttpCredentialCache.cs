using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;


namespace Hapikit.Credentials
{
    public class HttpCredentialCache : IEnumerable<HttpCredentials>
    {
        private readonly Dictionary<string,HttpCredentials> _Credentials = new Dictionary<string,HttpCredentials>(StringComparer.CurrentCultureIgnoreCase);
        private Regex _RealmParser = new Regex("Realm=\"(?<Realm>.*)\"",RegexOptions.IgnoreCase);
        public void Add(HttpCredentials info)
        {
            var key = MakeKey(info.OriginServer, info.Realm, info.AuthScheme);
            _Credentials.Add(key,info);    
        }

        private static string MakeKey(Uri originServer, string realm, string authScheme)
        {
            return originServer.AbsoluteUri + "|" + realm + "|" + authScheme;
        }

        public HttpCredentials GetMatchingCredentials(Uri originServer, IEnumerable<AuthenticationHeaderValue> challenges)
        {
            var matchingCredentials = from c in challenges
                                      let key = MakeKey(originServer, ParseRealm(c.Parameter), c.Scheme)
                                      let credentials = _Credentials.ContainsKey(key) ? _Credentials[key] : null
                                      where credentials != null
                                      orderby credentials.Priority
                                      select new { creds = credentials, challenge = c };

            var match = matchingCredentials.First();
            match.creds.LastChallengeParameters = match.challenge.Parameter;
            return match.creds;
        }

        private string ParseRealm(string parameters)
        {
            if (parameters == null) return "";
            var match = _RealmParser.Match(parameters);
            return match.Groups["Realm"].Value ?? "";
            
        }


        public IEnumerator<HttpCredentials> GetEnumerator()
        {
            return _Credentials.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}