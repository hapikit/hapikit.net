using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Hapikit.Credentials;

using Xunit;

namespace AuthTests
{
    public class BasicTests
    {
        [Fact]
        public void Add_basic_credential_to_cache()
        {
            var cache = new HttpCredentialCache
            {
                new BasicCredentials(new Uri("http://example.org"), username: "", password: "")
            };

            Assert.Equal(1, cache.Count());
        }


        [Fact]
        public void Retreive_credentials_from_cache()
        {
            var basicCredentials = new BasicCredentials(new Uri("http://example.org"), username: "", password: "");
            var cache = new HttpCredentialCache
            {
                new FooCredentials(new Uri("http://example.org")),
                basicCredentials,
                new BasicCredentials(new Uri("http://example.net"), username: "", password: ""),
                new BasicCredentials(new Uri("http://example.org"), username: "", password: "") {Realm = "foo"},
            };


            var creds = cache.GetMatchingCredentials(new Uri("http://example.org"), new[] { new AuthenticationHeaderValue("basic","foo") });

            Assert.Same(basicCredentials, creds);
            
        }

        [Fact]
        public void Retreive_credentials_from_cache_using_realm()
        {
            var basicCredentials = new BasicCredentials(new Uri("http://example.org"), username: "", password: "")
            {
                Realm = "foo"   
            };
            var cache = new HttpCredentialCache
            {
                basicCredentials,
            };


            var creds = cache.GetMatchingCredentials(new Uri("http://example.org"), new[] { new AuthenticationHeaderValue("basic", "Realm=\"foo\"") });

            Assert.Same(basicCredentials, creds);
            
        }



        [Fact]
        public void Reuse_last_used_credentials()
        {
            // Arrange
            var cache = new HttpCredentialCache
            {
                new BasicCredentials(new Uri("http://example.org"), username: "", password: "")
            };

            var authService = new CredentialService(cache);
            var request = new HttpRequestMessage(){RequestUri = new Uri("http://example.org")};
            authService.CreateAuthenticationHeaderFromChallenge(request, new[] { new AuthenticationHeaderValue("basic", "") });

            // Act
            var header = authService.CreateAuthenticationHeaderFromRequest(request);

            // Assert
            Assert.Equal("basic", header.Scheme);

        }

    }
}
