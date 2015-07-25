using Hapikit.Links;
using Hapikit.Templates;

namespace haveibeenpwnd.net.hapisdk
{
    public class DataClassLink : Link
    {
        public DataClassLink()
        {
            Template = new UriTemplate("https://haveibeenpwned.com/api/v2/dataclasses");
        }
    }
}