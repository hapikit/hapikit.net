using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hapikit.Vocabularies
{

    public class ApisJson
    {
        public string Name { get; set; } // string of human readable name for the collection of APIs
        public string Description { get; set; } //  human readable description of the collection of APIs.
        public Uri Url { get; set; } //  [Mandatory]: Web URL indicating the location of the latest version of this file
        public Uri Image { get; set; } // Image[Optional]: Web URL leading to an image to be used to represent the collection of

        public List<string> Tags { get; set; } // (collection) Optional]: this is a list of descriptive strings which identify the contents of the APIs.json file.Represented as an array. 
        public DateTime Created { get; set; } // [Mandatory]: date of creation of the file
        public DateTime Modified { get; set; } // [Mandatory]: date of last modification of the file
        public string SpecificationVersion { get; set; } //  [Mandatory]: version of the APIs.json specification in use.

        public List<ApisJsonApi> Apis { get; set; } = new List<ApisJsonApi>();
        public List<ApisJsonInclude> Includes { get; set; } = new List<ApisJsonInclude>();
        public List<ApisJsonContact> Maintainers { get; set; } = new List<ApisJsonContact>();


    }
    public class ApisJsonApi
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Uri Image { get; set; }
        public Uri HumanUrl { get; set; }
        public Uri BaseUrl { get; set; }
        public string Version { get; set; }
        public List<string> Tags { get; set; }
        public ApisJsonContact Contact { get; set; }
        public List<ApisJsonProperty> Properties { get; set; } = new List<ApisJsonProperty>();
    }

    public class ApisJsonProperty
    {
        public string Type { get; set; }
        public Uri Url { get; set; }
    }

    public class ApisJsonContact
    {
        public string Fn { get; set; }
        public string Email { get; set; }
        public Uri Url { get; set; }
        public string Org { get; set; }
        public string Adr { get; set; }
        public string Tel { get; set; }
        public string Twitter { get; set; }
        public string Github { get; set; }
        public Uri Photo { get; set; }
        public Uri VCard { get; set; }
    }

    public class ApisJsonInclude
    {
        public string Name { get; set; }
        public Uri Url { get; set; }
    }

}
