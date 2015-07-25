using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using haveibeenpwnd.net.hapisdk;
using Hapikit;
using Xunit;

namespace haveibeenpwnd.tests
{
    public class Class1
    {
        private HttpClient _httpClient;

        public Class1()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("hapikit-sdk-tests", "1.0"));
        }

        [Fact]
        public async Task GetBreaches()
        {
            var breachesLink = new BreachesLink();

            var response = await _httpClient.FollowLinkAsync(breachesLink);

            Assert.True(response.IsSuccessStatusCode);
            
            var result = await response.Content.ReadAsStringAsync();
            
        }

        [Fact]
        public async Task TestMe()
        {
            var breachAccountLink = new BreachedAccountLink()
            {
                Account = "darrel@tavis.ca"
            };

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("hapikit-sdk-tests", "1.0"));

            var response = await httpClient.FollowLinkAsync(breachAccountLink);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        }

        [Fact]
        public async Task TestBob()
        {
            var breachAccountLink = new BreachedAccountLink()
            {
                Account = "bob@gmail.com",
                Domain = "adobe.com",
                TruncateResponse = true
            };

            var response = await _httpClient.FollowLinkAsync(breachAccountLink);

            Assert.True(response.IsSuccessStatusCode);

            var result = await response.Content.ReadAsStringAsync();

        }

        [Fact]
        public async Task GetBreach()
        {
            var breachLink = new BreachLink()
            {
                Name = "adobe"
            };


            var response = await _httpClient.FollowLinkAsync(breachLink);

            Assert.True(response.IsSuccessStatusCode);

            var result = await response.Content.ReadAsStringAsync();

        }
    }
}
