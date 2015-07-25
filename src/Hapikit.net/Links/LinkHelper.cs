﻿using System;
using System.Linq;
using System.Reflection;


namespace Hapikit.Links
{
    /// <summary>
    /// Static helper methods for the Link class
    /// </summary>
    public static class LinkHelper
    {

        /// <summary>
        /// Helper method that reflects over a link type to find a LinkRelationTypeAttribute to return the link relation type name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static string GetLinkRelationTypeName<T>()
        {
            return GetLinkRelationTypeName(typeof (T));
        }

        /// <summary>
        /// Helper method that reflects over a link type to find a LinkRelationTypeAttribute to return the link relation type name
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static string GetLinkRelationTypeName(Type t)
        {
            var relation = "related";
            TypeInfo info = t.GetTypeInfo();
            
            Attribute[] attributes = info.GetCustomAttributes(typeof (LinkRelationTypeAttribute), false).ToArray();
            if (attributes.Length > 0)
            {
                var rel = (LinkRelationTypeAttribute) attributes[0];
                relation = rel.Name;
            }
            return relation;
        }
    }
}
