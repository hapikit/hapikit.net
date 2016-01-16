using System;
using System.Linq;
using System.Collections.Generic;
using Hapikit.RequestBuilders;
using Hapikit.Templates;

namespace Hapikit.Links
{
    public class AzureOAuthTokenLink : OAuth2TokenLink
    {
        [LinkParameter("tenantid")]
        public string TenantId { get; set; }


        public string Resource
        {
            get { return _BodyParameters["resource"]; }
            set { _BodyParameters["resource"] = value; }
        }


        public AzureOAuthTokenLink()
        {
            Template = new UriTemplate("https://login.windows.net/{tenantid}/oauth2/token");
            GrantType = "client_credentials";
        }
    }
}