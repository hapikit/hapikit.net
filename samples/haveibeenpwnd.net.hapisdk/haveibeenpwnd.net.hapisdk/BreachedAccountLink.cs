using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hapikit.Links;
using Hapikit.RequestBuilders;
using Hapikit.Templates;

namespace haveibeenpwnd.net.hapisdk
{
    public class BreachedAccountLink : Link
    {
        public string Account { get; set; }
        public string Domain { get; set; }
        
        [LinkParameter("truncateResponse",Default=false)]
        public bool TruncateResponse { get; set; }

        public BreachedAccountLink()
        {
            Template = new UriTemplate("https://haveibeenpwned.com/api/v2/breachedaccount/{account}{?truncateResponse,domain}");
        }
    }
}
