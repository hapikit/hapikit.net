using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hapikit.net.Tests
{
    public class VocabIntegrationTests
    {
        private ITestOutputHelper _output;
        public VocabIntegrationTests(ITestOutputHelper output)
        {
            _output = output;
        }
     //   [Fact]
        public void ParseVeryLargeSwaggerPaths()
        {
            var opsTerm = new VocabTerm<Operation>();

            var pathTerm = new VocabTerm<Path>();
            pathTerm.MapAnyObject<Operation>(opsTerm, (s, p) =>
            {
                return s.AddOperation(p, Guid.NewGuid().ToString());
            });

            var pathsTerm = new VocabTerm<OpenApiDocument>("paths");

            pathsTerm.MapAnyObject<Path>(pathTerm, (s, p) =>
            {
                return s.AddPath(p);
            });

            var rootTerm = new VocabTerm<OpenApiDocument>();
            rootTerm.MapObject<OpenApiDocument>(pathsTerm, (s) =>
            {
                return s;
            });

            var swaggerDoc = new OpenApiDocument();
            var sw = new Stopwatch();
            using (var stream = File.OpenRead(@"C:\Users\Darrel\Documents\Swagger\WebSites.json"))
            {

                sw.Start();

                JsonStreamingParser.ParseStream(stream, swaggerDoc, rootTerm);
                sw.Stop();
            };

            _output.WriteLine(swaggerDoc.Paths.Count.ToString());
            _output.WriteLine("That took : " + sw.ElapsedMilliseconds);

        }

       // [Fact]
        public void ParseVeryLargeSwaggerPaths2()
        {
            OpenApiDocument swaggerDoc;
            var sw = new Stopwatch();

            using (var stream = File.OpenRead(@"C:\Users\Darrel\Documents\Swagger\WebSites.json"))
            {
                sw.Start();
                swaggerDoc = JsonConvert.DeserializeObject<OpenApiDocument>(new StreamReader(stream).ReadToEnd());
                sw.Stop();
            };

            _output.WriteLine(swaggerDoc.Paths.Count.ToString());
            _output.WriteLine("That took : " + sw.ElapsedMilliseconds);

        }
    }
    }
