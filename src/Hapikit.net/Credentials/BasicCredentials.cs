using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;


namespace Hapikit.Credentials
{
    public class BasicCredentials : HttpCredentials
    {
        private readonly string _username;
        private readonly string _password;

        public BasicCredentials(Uri originServer, string username, string password) : base ("basic",originServer)
        {
            _username = username;
            _password = password;
        }

        public override AuthenticationHeaderValue CreateAuthHeader(HttpRequestMessage request)
        {
            return new AuthenticationHeaderValue(AuthScheme, Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Format("{0}:{1}", _username, _password))));
        }
    }
}