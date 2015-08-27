﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Hapikit.ResponseHandlers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace LinkTests
{
    public class MachineTests
    {

        [Fact]
        public async Task Handle404()
        {
            bool notfound = false;
            var machine = new HttpResponseMachine();

            machine.AddResponseAction(async (l, r) => { notfound = true; }, HttpStatusCode.NotFound);

            await machine.HandleResponseAsync("", new HttpResponseMessage(HttpStatusCode.NotFound));

            Assert.True(notfound);
        }

        [Fact]
        public async Task HandleUnknown4XX()
        {
            bool badrequest = false;
            var machine = new HttpResponseMachine();

            machine.AddResponseAction(async (l, r) => { badrequest = true; }, HttpStatusCode.BadRequest);

            await machine.HandleResponseAsync("", new HttpResponseMessage(HttpStatusCode.ExpectationFailed));

            Assert.True(badrequest);
        }

        [Fact]
        public async Task Handle200Only()
        {
            bool ok = false;
            var machine = new HttpResponseMachine();

            machine.AddResponseAction(async (l, r) => { ok = true; }, System.Net.HttpStatusCode.OK);

            await machine.HandleResponseAsync("", new HttpResponseMessage(HttpStatusCode.OK));

            Assert.True(ok);
        }

        [Fact]
        public async Task Handle200AndContentType()
        {
            // Arrange
            JToken root = null;
            var machine = new HttpResponseMachine();

            machine.AddResponseAction(async (l, r) =>
            {
                var text = await r.Content.ReadAsStringAsync();
                root = JToken.Parse(text);
            },HttpStatusCode.OK,null, new MediaTypeHeaderValue("application/json"));

            machine.AddResponseAction(async (l, r) => { }, HttpStatusCode.OK, null,new MediaTypeHeaderValue("application/xml"));

            var byteArrayContent = new ByteArrayContent(Encoding.UTF8.GetBytes("{\"hello\" : \"world\"}"));
            byteArrayContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            
            // Act
            await machine.HandleResponseAsync("", new HttpResponseMessage(HttpStatusCode.OK) { Content = byteArrayContent});

            // Assert
            Assert.NotNull(root);
        }


        [Fact]
        public async Task Handle200AndContentTypeUsingTypedMachine()
        {
            // Arrange
            var model = new Model<JToken>
            {
                Value = null
            };

            var machine = new HttpResponseMachine<Model<JToken>>(model);

            machine.AddResponseAction(async (m, l, r) =>
            {
                var text = await r.Content.ReadAsStringAsync();
                m.Value = JToken.Parse(text);
            }, HttpStatusCode.OK, null,new MediaTypeHeaderValue("application/json"));


            machine.AddResponseAction(async (m, l, r) => { }, HttpStatusCode.OK, null,new MediaTypeHeaderValue("application/xml"));

            var byteArrayContent = new ByteArrayContent(Encoding.UTF8.GetBytes("{\"hello\" : \"world\"}"));
            byteArrayContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            // Act
            await machine.HandleResponseAsync("", new HttpResponseMessage(HttpStatusCode.OK) { Content = byteArrayContent });

            // Assert
            Assert.NotNull(model.Value);
        }


        [Fact]
        public async Task Handle200AndContentTypeAndLinkRelation()
        {
            // Arrange
            JToken root = null;
            var machine = new HttpResponseMachine();

            // Fallback handler
            machine.AddResponseAction(async (l, r) =>
            {
                var text = await r.Content.ReadAsStringAsync();
                root = JToken.Parse(text);
            }, HttpStatusCode.OK); 

            // More specific handler
            machine.AddResponseAction(async (l, r) =>
            {
                var text = await r.Content.ReadAsStringAsync();
                root = JToken.Parse(text);
            }, HttpStatusCode.OK,linkRelation: "foolink", contentType: new MediaTypeHeaderValue("application/json"), profile: null);

            machine.AddResponseAction(async (l, r) => { }, HttpStatusCode.OK, linkRelation: "foolink", contentType: new MediaTypeHeaderValue("application/xml"), profile: null);

            var byteArrayContent = new ByteArrayContent(Encoding.UTF8.GetBytes("{\"hello\" : \"world\"}"));
            byteArrayContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            // A
            await machine.HandleResponseAsync("foolink", new HttpResponseMessage(HttpStatusCode.OK) { Content = byteArrayContent });

            // Assert
            Assert.NotNull(root);
        }

        [Fact]
        public async Task DispatchBasedOnStatusCodeMediaTypeAndProfile()
        {
            Person testPerson = null;

            var parserStore = new ParserStore();
            // Define method to translate response body into DOM for specified media type 
            parserStore.AddMediaTypeParser<JToken>("application/json", async (content) =>
            {
                var stream = await content.ReadAsStreamAsync();
                return JToken.Load(new JsonTextReader(new StreamReader(stream)));
            });

            // Define method to translate media type DOM into application domain object instance based on profile
            parserStore.AddProfileParser<JToken, Person>(new Uri("http://example.org/person"), (jt) =>
            {
                var person = new Person();
                var jobject = (JObject)jt;
                person.FirstName = (string)jobject["FirstName"];
                person.LastName = (string)jobject["LastName"];

                return person;
            });

            var machine = new HttpResponseMachine(parserStore);

            // Define action in HttpResponseMachine for all responses that return 200 OK and can be translated somehow to a Person
            machine.AddResponseAction<Person>((l, p) => { testPerson = p; }, HttpStatusCode.OK);

            // Create a sample body
            var jsonContent = new StringContent("{ \"FirstName\" : \"Bob\", \"LastName\" : \"Bang\"  }");
            jsonContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            jsonContent.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("profile", "\"http://example.org/person\""));
            
            // Create a sample response 
            var httpResponseMessage = new HttpResponseMessage()
            {
                Content = jsonContent
            };

            // Allow machine to dispatch response
            machine.HandleResponseAsync("", httpResponseMessage);

            Assert.NotNull(testPerson);
            Assert.Equal("Bob", testPerson.FirstName);
            Assert.Equal("Bang", testPerson.LastName);
        }


        [Fact]
        public async Task DispatchBasedOnStatusCodeAndLinkRelationAndParseProfile()
        {
            Model<Person> test = new Model<Person>();

            var parserStore = new ParserStore();
            // Define method to translate response body into DOM for specified media type 
            parserStore.AddMediaTypeParser<JToken>("application/json", async (content) =>
            {
                var stream = await content.ReadAsStreamAsync();
                return JToken.Load(new JsonTextReader(new StreamReader(stream)));
            });

            // Define method to translate media type DOM into application domain object instance based on profile
            parserStore.AddLinkRelationParser<JToken, Person>("person-link", (jt) =>
            {
                var person = new Person();
                var jobject = (JObject)jt;
                person.FirstName = (string)jobject["FirstName"];
                person.LastName = (string)jobject["LastName"];

                return person;
            });

            var machine = new HttpResponseMachine<Model<Person>>(test,parserStore);


            // Define action in HttpResponseMachine for all responses that return 200 OK and can be translated somehow to a Person
            machine.AddResponseAction<Person>((m, l, p) => { m.Value = p; }, HttpStatusCode.OK);

            // Create a sample body
            var jsonContent = new StringContent("{ \"FirstName\" : \"Bob\", \"LastName\" : \"Bang\"  }");
            jsonContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
      
            // Create a sample response 
            var httpResponseMessage = new HttpResponseMessage()
            {
                Content = jsonContent
            };

            // Allow machine to dispatch response
            machine.HandleResponseAsync("person-link", httpResponseMessage);

            Assert.NotNull(test.Value);
            Assert.Equal("Bob", test.Value.FirstName);
            Assert.Equal("Bang", test.Value.LastName);
        }



        [Fact]
        public async Task DispatchBasedOnMediaTypeWithParser()
        {
            JToken value = null;

            var parserStore = new ParserStore();

            parserStore.AddMediaTypeParser<JToken>("application/json", async (content) =>
            {
                var stream = await content.ReadAsStreamAsync();
                return JToken.Load(new JsonTextReader(new StreamReader(stream)));
            });

            var machine = new HttpResponseMachine(parserStore);

            machine.AddResponseAction<JToken>((l, jt) => { value = jt; }, HttpStatusCode.OK);

            var jsonContent = new StringContent("{ \"Hello\" : \"world\" }");
            jsonContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            machine.HandleResponseAsync("", new HttpResponseMessage()
            {
                Content = jsonContent
            });

            Assert.NotNull(value);
        }

    }

    public class Person
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
