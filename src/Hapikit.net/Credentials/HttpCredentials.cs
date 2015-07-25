using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Hapikit.Credentials
{
    public abstract class HttpCredentials     
    {
        public Uri OriginServer { get; private set; }

        public int Priority { get; set; }
        public string AuthScheme { get; private set; }
        public string Realm { get; set; }
        public string LastChallengeParameters { get; set; } 

        abstract public AuthenticationHeaderValue CreateAuthHeader(HttpRequestMessage request);

        protected HttpCredentials(string authScheme, Uri originServer)
        {
            AuthScheme = authScheme;
            OriginServer = originServer;
        }
    }
}