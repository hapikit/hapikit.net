using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Hapikit.ResponseHandlers
{
    public class ParserStore
    {
        private List<ParserInfo> _MediaTypeParsers = new List<ParserInfo>();
        private List<AppSemanticsParserInfo> _AppSemanticParsers = new List<AppSemanticsParserInfo>();

         public void AddMediaTypeParser<Target>(string mediaType, Func<HttpContent, Task<Target>> translator) where Target:class
        {
            _MediaTypeParsers.Add(new ParserInfo
            {
                MediaType = mediaType,
                TargetType = typeof(Target),
                Parse = async (c) => await translator(c)
            }); 
        }

       public void AddProfileParser<Source,Target>(Uri profile, Func<Source, Target> translator) where Target : class
       {
           _AppSemanticParsers.Add(new AppSemanticsParserInfo
           {
               Profile = profile,
               SourceType = typeof(Source),
               TargetType = typeof(Target),
               Parse = (s) => translator((Source)s)
           });
       }

       public void AddLinkRelationParser<Source, Target>(string linkrelation, Func<Source, Target> translator) where Target : class
       {
            _AppSemanticParsers.Add(new AppSemanticsParserInfo
           {
               LinkRelation = linkrelation,
               SourceType = typeof(Source),
               TargetType = typeof(Target),
               Parse = (s) => translator((Source)s)
           });
       }
    
        internal IEnumerable<ParserInfo> GetMediaTypeParsers<Target>()
        {
 	         return _MediaTypeParsers.Where(pi => pi.TargetType == typeof(Target)).ToList();

        }

        internal IEnumerable<ParserPair> GetSemanticParsers<Target>(string linkRelation)
        {
            return _AppSemanticParsers
                           .Where(pi => pi.TargetType == typeof(Target)
                               && (linkRelation == null || linkRelation == pi.LinkRelation))
                           .SelectMany(pp => _MediaTypeParsers.Where(pi => pi.TargetType == pp.SourceType)
                                               .Select(mt => new ParserPair { MediaTypeParser = mt, ProfileParser = pp }));
        }
    }
    
        internal struct ParserInfo
        {
            public string MediaType;
            public string Profile;
            public Type TargetType;
            public Func<HttpContent, Task<object>> Parse;

        }
        internal struct ParserPair
        {
            public ParserInfo MediaTypeParser;
            public AppSemanticsParserInfo ProfileParser;
        }
        internal struct AppSemanticsParserInfo
        {
            public Uri Profile;
            public string LinkRelation;
            public Type SourceType;
            public Type TargetType;
            public Func<object, object> Parse;
        }
}
