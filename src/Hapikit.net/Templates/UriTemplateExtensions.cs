using System.Collections.Generic;
using System.Reflection;


namespace Hapikit.Templates
{
    public static class UriTemplateExtensions
    {
        public static UriTemplate AddParameter(this UriTemplate template, string name, object value)
        {
            template.SetParameter(name, value);

            return template;
        }

        public static UriTemplate AddParameters(this UriTemplate template, object parametersObject)
        {

            if (parametersObject != null)
            {
                TypeInfo type = parametersObject.GetType().GetTypeInfo();

                foreach (var propinfo in type.DeclaredProperties)
                {
                    template.SetParameter(propinfo.Name, propinfo.GetValue(parametersObject, null));
                }
            }

            return template;
        }
        public static UriTemplate AddParameters(this UriTemplate uriTemplate, IDictionary<string, object> linkParameters)
        {
            if (linkParameters != null)
            {
                foreach (var parameter in linkParameters)
                {
                    uriTemplate.SetParameter(parameter.Key, parameter.Value);
                }
            }
            return uriTemplate;
        }
    }
}
