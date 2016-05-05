using System;
using System.Collections.Generic;

namespace Hapikit.Vocabularies
{
    public static class ApisJsonVocab
    {
        public static VocabTerm<ApisJson> Create()
        {
            var vocab = new VocabTerm<ApisJson>();
            vocab.MapProperty<string>("name", (s, o) => s.Name = o );
            vocab.MapProperty<string>("description", (s, o) => s.Description = o );
            vocab.MapProperty<string>("url", (s, o) => s.Url = new Uri(o));
            vocab.MapProperty<string>("image", (s, o) => s.Image = new Uri(o));
            vocab.MapProperty<string>("modified", (s, o) =>s.Modified = DateTime.Parse(o));
            vocab.MapProperty<string>("created", (s, o) =>s.Created = DateTime.Parse(o));
            vocab.MapProperty<string>("tags", (s, o) =>
            {
                if (s.Tags == null)
                    s.Tags = new List<string>();
                s.Tags.Add(o);
            });

            vocab.MapProperty<string>("specificationVersion", (s, o) => s.SpecificationVersion = o );

            var apivocab = new VocabTerm<ApisJsonApi>("apis");
            apivocab.MapProperty<string>("name", (s, o) => s.Name = o );
            apivocab.MapProperty<string>("description", (s, o) => s.Name = o );
            apivocab.MapProperty<string>("humanUrl", (s, o) => s.HumanUrl = new Uri(o) );
            apivocab.MapProperty<string>("baseUrl", (s, o) => s.BaseUrl = new Uri(o) );
            apivocab.MapProperty<string>("version", (s, o) => s.Version = o );
            apivocab.MapProperty<string>("image", (s, o) => s.Image = new Uri(o) );

            apivocab.MapProperty<string>("tags", (s, o) =>
            {
                if (s.Tags == null)
                    s.Tags = new List<string>();
                s.Tags.Add(o);
            }

            );
            vocab.MapObject<ApisJsonApi>(apivocab, (s) =>
            {
                var api = new ApisJsonApi();
                s.Apis.Add(api);
                return api;
            }

            );
            // Properties
            var propertyTerm = new VocabTerm<ApisJsonProperty>("properties");
            propertyTerm.MapProperty<string>("url", (s, o) => s.Url = new Uri(o));
            propertyTerm.MapProperty<string>("type", (s, o) => s.Type = o );
            apivocab.MapObject<ApisJsonProperty>(propertyTerm, (s) =>
            {
                var property = new ApisJsonProperty();
                s.Properties.Add(property);
                return property;
            }

            );
            // Contact
            var contactTerm = new VocabTerm<ApisJsonContact>("contact");
            contactTerm.MapProperty<string>("fn", (s, o) => s.Url = new Uri(o));
            contactTerm.MapProperty<string>("email", (s, o) => s.Email = o);
            contactTerm.MapProperty<string>("url", (s, o) => s.Url = new Uri(o));
            contactTerm.MapProperty<string>("org", (s, o) => s.Org = o);
            contactTerm.MapProperty<string>("adr", (s, o) => s.Adr = o);
            contactTerm.MapProperty<string>("tel", (s, o) => s.Tel = o);
            contactTerm.MapProperty<string>("x-twitter", (s, o) => s.Twitter = o);
            contactTerm.MapProperty<string>("x-github", (s, o) => s.Github = o);
            contactTerm.MapProperty<string>("photo", (s, o) => s.Photo = new Uri(o));
            contactTerm.MapProperty<string>("vcard", (s, o) => s.VCard = new Uri(o));
            apivocab.MapObject<ApisJsonContact>(contactTerm, (s) =>
            {
                s.Contact = new ApisJsonContact();
                return s.Contact;
            }

            );
            // Include
            var includeTerm = new VocabTerm<ApisJsonInclude>("include");
            includeTerm.MapProperty<string>("name", (s, o) => s.Name = o);
            includeTerm.MapProperty<string>("url", (s, o) => s.Url = new Uri(o));
            vocab.MapObject<ApisJsonInclude>(includeTerm, (s) =>
            {
                var include = new ApisJsonInclude();
                s.Includes.Add(include);
                return include;
            }

            );
            //Maintainers
            var maintainers = contactTerm.Clone<ApisJson>("maintainers");
            vocab.MapObject<ApisJsonContact>(maintainers, (s) =>
            {
                var contact = new ApisJsonContact();
                s.Maintainers.Add(contact);
                return contact;
            }

            );
            return vocab;
        }
    }
}