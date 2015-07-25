using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;


namespace Hapikit.Credentials
{
    public class CredentialService 
    {
        private readonly HttpCredentialCache _credentialCache;
        private readonly ConcurrentDictionary<Uri, HttpCredentials> _lastMatchedCredentials = new ConcurrentDictionary<Uri, HttpCredentials>();

        public CredentialService(HttpCredentialCache credentialCache)
        {
            _credentialCache = credentialCache;
        }

        public AuthenticationHeaderValue CreateAuthenticationHeaderFromRequest(HttpRequestMessage request)
        {
            var originServer = GetOriginUri(request);

            if (_lastMatchedCredentials.ContainsKey(originServer))
            {
                return _lastMatchedCredentials[originServer].CreateAuthHeader(request);
            }
            return null;
        }

        public AuthenticationHeaderValue CreateAuthenticationHeaderFromChallenge(HttpRequestMessage request,IEnumerable<AuthenticationHeaderValue> challenges)
        {
            var originServer = GetOriginUri(request);

            var creds = _credentialCache.GetMatchingCredentials(originServer, challenges);
            
            if (creds != null)
            {
                _lastMatchedCredentials[originServer] = creds;
                return creds.CreateAuthHeader(request);
            }

            return null;
        }

        public static Uri GetOriginUri(HttpRequestMessage request)
        {
            return new Uri(request.RequestUri.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped));
        }

    }
}