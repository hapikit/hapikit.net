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

    public static class ApisJsonVocab
    {
        public static VocabTerm<ApisJson> Create()
        {
            var vocab = new VocabTerm<ApisJson>();

            vocab.MapProperty<string>("name", (s, o) => { s.Name = o; });
            vocab.MapProperty<string>("description", (s, o) => { s.Description = o; });
            vocab.MapProperty<string>("url", (s, o) => { s.Url = new Uri(o); });
            vocab.MapProperty<string>("image", (s, o) => { s.Image = new Uri(o); });
            vocab.MapProperty<string>("modified", (s, o) => { s.Modified = DateTime.Parse(o); });
            vocab.MapProperty<string>("created", (s, o) => { s.Created = DateTime.Parse(o); });
            vocab.MapProperty<string>("tags", (s, o) => {
                if (s.Tags == null) s.Tags = new List<string>();
                s.Tags.Add(o);
            });
            vocab.MapProperty<string>("specificationVersion", (s, o) => { s.SpecificationVersion = o; });


            var apivocab = new VocabTerm<ApisJsonApi>("apis");
            apivocab.MapProperty<string>("name", (s, o) => { s.Name = o; });
            apivocab.MapProperty<string>("description", (s, o) => { s.Name = o; });
            apivocab.MapProperty<string>("humanUrl", (s, o) => { s.HumanUrl = new Uri(o); });
            apivocab.MapProperty<string>("baseUrl", (s, o) => { s.BaseUrl = new Uri(o); });
            apivocab.MapProperty<string>("version", (s, o) => { s.Version = o; });
            apivocab.MapProperty<string>("image", (s, o) => { s.Image = new Uri(o); });
            apivocab.MapProperty<string>("tags", (s, o) => {
                if (s.Tags == null) s.Tags = new List<string>();
                s.Tags.Add(o);
            });

            vocab.MapObject<ApisJsonApi>(apivocab, (s) =>
            {
                var api = new ApisJsonApi();
                s.Apis.Add(api);
                return api;
            });

            // Properties
            var propertyTerm = new VocabTerm<ApisJsonProperty>("properties");
            propertyTerm.MapProperty<string>("url", (s, o) => { s.Url = new Uri(o); });
            propertyTerm.MapProperty<string>("type", (s, o) => { s.Type = o; });

            apivocab.MapObject<ApisJsonProperty>(propertyTerm, (s) =>
            {
                var property = new ApisJsonProperty();
                s.Properties.Add(property);
                return property;
            });

            // Contact
            var contactTerm = new VocabTerm<ApisJsonContact>("contact");
            contactTerm.MapProperty<string>("fn", (s, o) =>  s.Url = new Uri(o));
            contactTerm.MapProperty<string>("email", (s, o) =>  s.Email = o);
            contactTerm.MapProperty<string>("url", (s, o) =>  s.Url = new Uri(o));
            contactTerm.MapProperty<string>("org", (s, o) =>  s.Org = o);
            contactTerm.MapProperty<string>("adr", (s, o) =>  s.Adr = o);
            contactTerm.MapProperty<string>("tel", (s, o) =>  s.Tel = o);
            contactTerm.MapProperty<string>("x-twitter", (s, o) =>  s.Twitter = o);
            contactTerm.MapProperty<string>("x-github", (s, o) => s.Github = o);
            contactTerm.MapProperty<string>("photo", (s, o) =>  s.Photo = new Uri(o));
            contactTerm.MapProperty<string>("vcard", (s, o) =>  s.VCard = new Uri(o));

            apivocab.MapObject<ApisJsonContact>(contactTerm, (s) => {
                s.Contact = new ApisJsonContact();
                return s.Contact;
            });

            // Include
            var includeTerm = new VocabTerm<ApisJsonInclude>("include");
            includeTerm.MapProperty<string>("name", (s, o) => s.Name = o);
            includeTerm.MapProperty<string>("url", (s, o) => s.Url = new Uri(o));

            vocab.MapObject<ApisJsonInclude>(includeTerm, (s) =>
            {
                var include = new ApisJsonInclude();
                s.Includes.Add(include);
                return include;
            });

            // Maintainers
         
            //vocab.MapObject<ApisJsonContact>("maintainers",contactTerm, (s) =>
            //{
            //    var contact = new ApisJsonContact();
            //    s.Maintainers.Add(contact);
            //    return contact;
            //});

            return vocab;
        }
    }
}
