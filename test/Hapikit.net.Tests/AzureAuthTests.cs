using Hapikit.Links;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.Net.Http;

namespace AuthTests
{
    public class AzureAuthTests
    {
        [Fact]
        public void GetAzureToken()
        {

            var tokenLink = new AzureOAuthTokenLink()
            {
                TenantId = "foo",
                Resource = "https://management.core.windows.net/",
                ClientId = "ack",
                ClientSecret = "bar"
            };

            var request = tokenLink.CreateRequest();

            Assert.Equal("https://login.windows.net/foo/oauth2/token", request.RequestUri.AbsoluteUri);
            var body = request.Content.ReadAsStringAsync().Result;
            Assert.Equal("grant_type=client_credentials&resource=https%3A%2F%2Fmanagement.core.windows.net%2F&client_id=ack&client_secret=bar", body);

        }

        //[Fact]
        public async Task GetAzureTokenForReal()
        {

            var tokenLink = new AzureOAuthTokenLink()
            {
                TenantId = "<TenantId>", // Go to Azure portal->AD-Applications and click view Endpoints at the bottom. Take the guid from the first path segment.
                Resource = "https://management.core.windows.net/",
                ClientId = "<ClientId>",
                ClientSecret = "<Client Secret>"
            };

            var httpClient = new HttpClient();

            var response = await httpClient.SendAsync(tokenLink.CreateRequest());

            string token = null;
            if (response.IsSuccessStatusCode)
            {
                var tokenResponse = AzureOAuthTokenLink.ParseTokenBody(await response.Content.ReadAsStringAsync());
                token = tokenResponse.AccessToken;
            } else
            {
                var errorResponse = AzureOAuthTokenLink.ParseErrorBody(await response.Content.ReadAsStringAsync());
            }

            Assert.NotNull(token);
        }
    }

}
