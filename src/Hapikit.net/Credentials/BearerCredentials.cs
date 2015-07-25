using System;
using System.Net.Http;
using System.Net.Http.Headers;


namespace Hapikit.Credentials
{
    public class BearerCredentials : HttpCredentials
    {
        private readonly string _token;

        public BearerCredentials(Uri originServer, string token) : base("bearer", originServer)
        {
            _token = token;
        }

        public override AuthenticationHeaderValue CreateAuthHeader(HttpRequestMessage request)
        {
            return new AuthenticationHeaderValue(AuthScheme, _token);
        }
    }
}