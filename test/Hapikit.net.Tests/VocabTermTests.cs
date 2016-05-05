using Hapikit.Tests;
using Hapikit.Vocabularies;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tavis;
using Xunit;
using Xunit.Abstractions;

namespace Hapikit.net.Tests
{
    public class VocabTermTests
    {
        readonly ITestOutputHelper output;
        DiagnosticListener _listener;

        public VocabTermTests(ITestOutputHelper output)
        {
            this.output = output;
            _listener = new DiagnosticListener("Testing");
            _listener.Subscribe(new ConsoleLogger(this.output));
            JsonStreamingParser.DiagSource = _listener;

        }

        [Fact]
        public void ParseTerm()
        {

            // Arrange
            var doc = new JObject(new JProperty("foo", "bar"), new JProperty("baz", 22));
            var localObject = new LocalType();

            // Act
            var rootMap = new VocabTerm<LocalType>();
            rootMap.MapProperty<string>("foo", (s, o) => s.Foo = o);
            rootMap.MapProperty<int>("baz", (s, o) => s.Baz = o);

            JsonStreamingParser.ParseStream(doc.ToMemoryStream(), localObject, rootMap);

            // Assert
            Assert.Equal("bar", localObject.Foo);
            Assert.Equal(22, localObject.Baz);
        }


        [Fact]
        public void ParseTermInChild()
        {
            // Arrange
            var doc = new JObject(new JProperty("foo", "bar"),
                                    new JProperty("child",
                                            new JObject(new JProperty("Ick", "much"), new JProperty("Ock", "wow"))));
            var localObject = new LocalType();

            // Act
            var root = new VocabTerm<LocalType>();
            var child = new VocabTerm<LocalChildType>("child");

            child.MapProperty<string>("Ick", (s, o) => s.Ick = o);
            child.MapProperty<string>("Ock", (s, o) => s.Ock = o);

            root.MapObject<LocalChildType>(child, (s) =>
            {
                var subject = s;
                subject.Child = new LocalChildType();
                return subject.Child;
            });

            JsonStreamingParser.ParseStream(doc.ToMemoryStream(), localObject, root);

            // Assert
            Assert.Equal("much", localObject.Child.Ick);
            Assert.Equal("wow", localObject.Child.Ock);
        }

        [Fact]
        public void GetGlobalTerm()
        {
            var termFoo = new VocabTerm("foo");
            var termBar = new VocabTerm("bar");
            var termBaz = new VocabTerm("baz");

            var rootTerm = new VocabTerm(null); 
            rootTerm.AddChild(termFoo.Term,termFoo);
            rootTerm.AddChild(termBar.Term,termBar);
            rootTerm.AddChild(termBaz.Term,termBaz);

            Assert.Equal(termFoo, rootTerm.Find("foo"));
            Assert.Null(rootTerm.Find("fooz"));
            Assert.Equal(termBaz, rootTerm.Find("baz"));

        }




        [Fact]
        public void ParseProblemDocument()
        {
            var stream = this.GetType().Assembly.GetManifestResourceStream("Hapikit.net.Tests.problem.json");
            Func<string, Uri> typeProducer = (o) => new Uri(o); 
            var vocab = new VocabTerm<ProblemDocument>();
            
            vocab.MapProperty<string>("type", (s, o) => { s.ProblemType = typeProducer(o); });
            vocab.MapProperty<string>("title", (s, o) => { s.Title = o; });
            vocab.MapProperty<string>("detail", (s, o) => { s.Detail = o; });
            vocab.MapProperty<string>("instance", (s, o) => { s.ProblemInstance = new Uri(o,UriKind.RelativeOrAbsolute); });


            var problem = new Tavis.ProblemDocument();
            JsonStreamingParser.ParseStream(stream, problem, vocab);

            Assert.Equal("https://example.com/probs/out-of-credit", problem.ProblemType.OriginalString);
            Assert.Equal("You do not have enough credit.", problem.Title);
        }

        [Fact]
        public void ParseAPIsJsonDocument()
        {
            var stream = this.GetType().Assembly.GetManifestResourceStream(this.GetType(),"apis.json");

            var vocab = ApisJsonVocab.Create();

            //new VocabTerm<ApisJson>(null);

            //vocab.MapProperty<string>("name", (s, o) => { s.Name = o; });
            //vocab.MapProperty<string>("url", (s, o) => { s.Url = new Uri(o); });
            //vocab.MapProperty<string>("image", (s, o) => { s.Image = new Uri(o); });
            //vocab.MapProperty<string>("modified", (s, o) => { s.Modified = DateTime.Parse(o); });
            //vocab.MapProperty<string>("created", (s, o) => { s.Created = DateTime.Parse(o); });
            //vocab.MapProperty<string>("tags", (s, o) => {
            //    if (s.Tags == null) s.Tags = new List<string>();
            //    s.Tags.Add(o); });
            //vocab.MapProperty<string>("specificationVersion", (s, o) => { s.SpecificationVersion = o; });


            //var apivocab = new VocabTerm<ApisJsonApi>("apis");
            //apivocab.MapProperty<string>("name", (s, o) => { s.Name = o; });
            //apivocab.MapProperty<string>("description", (s, o) => { s.Name = o; });
            //apivocab.MapProperty<string>("humanUrl", (s, o) => { s.HumanUrl = new Uri(o); });
            //apivocab.MapProperty<string>("baseUrl", (s, o) => { s.BaseUrl = new Uri(o); });
            //apivocab.MapProperty<string>("image", (s, o) => { s.Image = new Uri(o); });
            //apivocab.MapProperty<string>("tags", (s, o) => {
            //    if (s.Tags == null) s.Tags = new List<string>();
            //    s.Tags.Add(o);
            //});

            //vocab.MapObject<ApisJsonApi>(apivocab, (s) =>
            //{
            //    if (s.Apis == null)
            //    {
            //        s.Apis = new List<ApisJsonApi>();
            //    }
            //    var api = new ApisJsonApi();
            //    s.Apis.Add(api);
            //    return api;
            //});

            //var propertyTerm = new VocabTerm<ApisJsonProperty>("properties");
            //propertyTerm.MapProperty<string>("url", (s, o) => { s.Url = new Uri(o);});
            //propertyTerm.MapProperty<string>("type", (s, o) => { s.Type = o;});


            //apivocab.MapObject<ApisJsonProperty>(propertyTerm, (s) =>
            //{ 
            //    if (s.Properties == null)
            //    {
            //        s.Properties = new List<ApisJsonProperty>();
            //    }
            //    var property = new ApisJsonProperty();
            //    s.Properties.Add(property);
            //    return property;
            //});

            
            var apis = new ApisJson();
            JsonStreamingParser.ParseStream(stream, apis, vocab);

            Assert.Equal("API Evangelist", apis.Name);
            Assert.Equal(2,apis.Apis.Count());
        }

        [Fact]
        public void ParseAPIsJsonDocumentMinimal()
        {
            var stream = this.GetType().Assembly.GetManifestResourceStream(this.GetType(), "apis.json");

            var rootMap = new VocabTerm<ApisJson>(null);
            rootMap.MapProperty<string>("name",         (s, o) => s.Name = o );
            rootMap.MapProperty<string>("url",          (s, o) => s.Url = new Uri(o));

            var apiMap = new VocabTerm<ApisJsonApi>("apis");
            apiMap.MapProperty<string>("name",          (s, o) => s.Name = o);
            apiMap.MapProperty<string>("description",   (s, o) => s.Name = o);
            apiMap.MapProperty<string>("baseUrl",       (s, o) => s.BaseUrl = new Uri(o));

            rootMap.MapObject<ApisJsonApi>(apiMap, (s) =>
            {
                if (s.Apis == null)
                {
                    s.Apis = new List<ApisJsonApi>();
                }
                var api = new ApisJsonApi();
                s.Apis.Add(api);
                return api;
            });

            var apis = new ApisJson();
            JsonStreamingParser.ParseStream(stream, apis, rootMap);

            Assert.Equal("API Evangelist", apis.Name);
            Assert.Equal(2, apis.Apis.Count());
        }


        [Fact]
        public void ParseSwaggerPaths()
        {

            var opsTerm = new VocabTerm<Operation>();

            var pathTerm = new VocabTerm<Path>();
            pathTerm.MapAnyObject<Operation>(opsTerm, (s, p) => {
                return s.AddOperation(p, Guid.NewGuid().ToString());
            });

            var pathsTerm = new VocabTerm<OpenApiDocument>("paths");

            pathsTerm.MapAnyObject<Path>(pathTerm,(s,p) => {
                return s.AddPath(p);
            });

            var rootTerm = new VocabTerm<OpenApiDocument>();
            rootTerm.MapObject<OpenApiDocument>(pathsTerm, (s) =>
             {
                 return s;
             });



            var stream = this.GetType().Assembly
     .GetManifestResourceStream(this.GetType(), "forecast.io.swagger.json");

            var swaggerDoc = new OpenApiDocument();

            JsonStreamingParser.ParseStream(stream, swaggerDoc, rootTerm);

            Assert.Equal(1, swaggerDoc.Paths.Count);
            Assert.Equal(1, swaggerDoc.Paths.First().Value.Operations.Count());
        }

        [Fact]
        public void ParseCompleteSwagger()
        {

            var vocab = OpenApiVocab.Create();
            
            var stream = this.GetType().Assembly
                .GetManifestResourceStream(this.GetType(), "forecast.io.swagger.json");

            var swaggerDoc = new OpenApiDocument();

            JsonStreamingParser.ParseStream(stream, swaggerDoc, vocab);

            Assert.Equal("2.0", swaggerDoc.Version);
            Assert.Equal(1, swaggerDoc.Schemes.Count);
            Assert.Equal("https", swaggerDoc.Schemes.First());

            Assert.Equal(1, swaggerDoc.Paths.Count );
            Assert.Equal(swaggerDoc.Paths.Keys.First(), "/forecast/{apiKey}/{latitude},{longitude}");
            var path = swaggerDoc.Paths.Values.First();
            Assert.Equal(1, path.Operations.Count);
            var operation = path.Operations.Values.First();
            Assert.Equal("Forecast", operation.Id);

            Assert.False(String.IsNullOrEmpty(swaggerDoc.Info.Description));
            Assert.False(String.IsNullOrEmpty(swaggerDoc.Info.Title));
            Assert.True(String.IsNullOrEmpty(swaggerDoc.Info.Version));
            

        }
    }




    // Objects with unknown keys
    // Arrays of Objects
    public class LocalType
    {
        public string Foo { get; set; }
        public int Baz { get; set; }
        public LocalChildType Child { get; set; }

    }

    public class LocalChildType
    {
        public string Ick { get; set; }
        public string Ock { get; set; }
    }

    public static class JObjectExtensions
    {
        public static MemoryStream ToMemoryStream(this JObject jObject)
        {
            var stream = new MemoryStream();

            var sw = new StreamWriter(stream);
            sw.Write(jObject.ToString());
            sw.Flush();

            stream.Position = 0;
            return stream;
        }
        public static MemoryStream ToMemoryStream(this string somestring)
        {
            var stream = new MemoryStream();

            var sw = new StreamWriter(stream);
            sw.Write(somestring);
            sw.Flush();

            stream.Position = 0;
            return stream;
        }
    }

}
