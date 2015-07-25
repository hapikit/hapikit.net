using System;
using System.Net.Http;

namespace Hapikit.Links
{
    public class Hint
    {
        public Func<Hint, HttpRequestMessage, HttpRequestMessage> ConfigureRequest { get; set; }


        public string Name { get; set; }
        public string Content { get; set; }  // Json document
    }
}