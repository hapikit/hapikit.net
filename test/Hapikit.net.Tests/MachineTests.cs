using System;
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
        public async Task AddMediaTypeTranslator()
        {
            JToken value = null;
            
            var machine = new HttpResponseMachine();
            
            machine.AddMediaTypeParser<JToken>("application/json", async (content) =>
            {
                var stream = await content.ReadAsStreamAsync();
                return JToken.Load(new JsonTextReader(new StreamReader(stream)));
            });
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
}
