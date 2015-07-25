using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;


namespace Hapikit.Credentials
{
    public class AuthMessageHandler : DelegatingHandler
    {
      
        private readonly CredentialService _credentialService;

        public AuthMessageHandler(HttpMessageHandler innerHandler, CredentialService credentialService)
        {
            InnerHandler = innerHandler;
            _credentialService = credentialService;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            
            // Attempt to reuse last credentials used for the same origin server  (Preauthenticate)
            if (request.Headers.Authorization == null)
            {
                request.Headers.Authorization = _credentialService.CreateAuthenticationHeaderFromRequest(request);
            }

            var response = await base.SendAsync(request, cancellationToken);
            
            // If request failed and challenge issued
            if (response.StatusCode == HttpStatusCode.Unauthorized && response.Headers.WwwAuthenticate.Count > 0)
            {
                // Can't automatically resend the request if it is not buffered.
                // Not sure how to detect this

                var authHeader = _credentialService.CreateAuthenticationHeaderFromChallenge(request, response.Headers.WwwAuthenticate);
                if (authHeader != null)
                {
                    var newRequest = await CopyRequest(request);
                    newRequest.Headers.Authorization = authHeader;

                    // Resend request with auth header based on challenge
                    response = await base.SendAsync(newRequest, cancellationToken);
                }
            }
            return response;
        }

        private static async Task<HttpRequestMessage> CopyRequest(HttpRequestMessage oldRequest) {
            var newrequest = new HttpRequestMessage(oldRequest.Method, oldRequest.RequestUri);
            
            foreach (var header in oldRequest.Headers) {
                newrequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
            foreach (var property in oldRequest.Properties) {
                newrequest.Properties.Add(property);
            }
            
            if (oldRequest.Content != null)
            {
                var stream = await oldRequest.Content.ReadAsStreamAsync();
                if (stream.Position != 0)
                {
                    if (!stream.CanSeek) throw new Exception("Cannot resend this request as the content is not re-readable");
                    stream.Position = 0;
                }

                newrequest.Content = new StreamContent(stream);
            }

            return newrequest;
        }

    }
}
