using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hapikit.Links;
using Hapikit.RequestBuilders;
using Hapikit.Templates;

namespace haveibeenpwnd.net.hapisdk
{
    public class BreachesLink : Link
    {
        public BreachesLink()
        {
            Template = new UriTemplate("https://haveibeenpwned.com/api/breaches");
            AddRequestBuilder(new InlineRequestBuilder(r =>
            {
                r.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/vnd.haveibeenpwned.v2+json"));
                return r;
            }));
        }
    }
}
