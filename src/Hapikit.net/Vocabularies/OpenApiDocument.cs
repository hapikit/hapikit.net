using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hapikit
{
    public class Schema
    {

    }
    public class SecurityScheme
    {

    }
    public class Tag { }
    public class ExternalDoc { }

    public class OpenApiDocument 
    {
        public string Version { get; set; } // Swagger
        public Info Info { get; set; }
        public string Host { get; set; }
        public string BasePath { get; set; }
        public List<string> Schemes { get; set; }
        public List<string> Consumes { get; set; }
        public List<string> Produces { get; set; }

        public Dictionary<string,Path> Paths { get; set; }
        public Dictionary<string, Schema> Definitions { get; set; }
        public Dictionary<string, Parameter> Parameters { get; set; }
        public Dictionary<string, Response> Responses { get; set; }
        public Dictionary<string, SecurityScheme> SecurityDefinitions { get; set; }
        public Dictionary<string, String[]> Security { get; set; }
        public Dictionary<string, Tag> Tags { get; set; }
        public Dictionary<string, ExternalDoc> ExternalDocs { get; set; }
        public Dictionary<string, string> Extensions { get; set; }


        public OpenApiDocument()
        {
            Paths = new Dictionary<string, Path>();
        }

        public Path AddPath(string path, Action<Path> configure = null)
        {
            var pathInfo = new Path();
            Paths.Add(path, pathInfo);
            if (configure != null) configure(pathInfo);
            return pathInfo;
        }

        public override string ToString()
        {
            var jObject = new JObject(new JProperty("swagger", "2.0"),
                            new JProperty("paths", new JObject(Paths.Select(
                                    p => new JProperty(p.Key, PathToJObject(p.Value))))));

            return jObject.ToString();
        }

        private static JObject PathToJObject(Path p)
        {
            return new JObject(p.Operations.Select(
                                  op => new JProperty(op.Key, OpToJObject(op.Value))));
        }

        private static JObject OpToJObject(Operation op)
        {
            return new JObject(new JProperty("operationId", op.Id));
        }

    }

    public class Contact
    {
        public string Name { get; set; }
        public Uri Url { get; set; }
        public string Email { get; set; }
        public Dictionary<string,string> Extensions { get; set; }
    }


    public class License
    {
        public string Name { get; set; }
        public Uri Url { get; set; }
        public Dictionary<string, string> Extensions { get; set; }
    }

    public class Info
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string TermsOfService { get; set; }
        public Contact Contact { get; set; }
        public License License { get; set; }
        public string Version { get; set; }
        public Dictionary<string, string> Extensions { get; set; }
    }

    public class Path
    {
        public Path()
        {
            Operations = new Dictionary<string, Operation>();
        }
        public Dictionary<string, Operation> Operations {get;set;}
        public string XController { get; set; }

        public Operation AddOperation(string method,string id, Action<Operation> configure = null)
        {
            var op = new Operation() { Id = id };
            Operations.Add(method, op);
            if (configure != null) configure(op);
            return op;
        }
    }

    public class Operation
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public List<Parameter> Parameters {get;set;}
        public List<Response> Responses { get; set; }

    }

    public class Parameter
    {

    }

    public class Response
    {

    }
}
