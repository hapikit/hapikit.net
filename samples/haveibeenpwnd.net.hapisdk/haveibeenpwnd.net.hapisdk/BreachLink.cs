using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hapikit.Links;

namespace haveibeenpwnd.net.hapisdk
{
    public class BreachLink : Link
    {
        public string Name { get; set; }

        public BreachLink()
        {
            Template = new Hapikit.Templates.UriTemplate("https://haveibeenpwned.com/api/v2/breach/{name}");
        }
        
    }
}
