using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Hapikit.Credentials;
using HawkNet;


namespace AuthTests
{
    public class HawkCredentials : HttpCredentials
    {
        private HawkCredential _hawkCredential { get; set; }

        public HawkCredentials(Uri originServer, HawkCredential hawkCredential) : base("Hawk",originServer)
        {
            _hawkCredential = hawkCredential;
        }

        public override AuthenticationHeaderValue CreateAuthHeader(HttpRequestMessage request)
        {
            var host = (request.Headers.Host != null) ? request.Headers.Host :
                request.RequestUri.Host +
                    ((request.RequestUri.Port != 80) ? ":" + request.RequestUri.Port : "");

            var hawk = Hawk.GetAuthorizationHeader(host,
                request.Method.ToString(),
                request.RequestUri,
                _hawkCredential);

            return new AuthenticationHeaderValue("Hawk", hawk); 
        }
    }
}