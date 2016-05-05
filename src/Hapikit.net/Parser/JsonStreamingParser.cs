using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Hapikit
{
    

    public class JsonStreamingParser
    {
        public static DiagnosticSource DiagSource {
            get; set;
        }

        public static void ParseStream(Stream stream, object rootSubject, VocabTerm rootTerm)
        {
            var reader = new JsonTextReader(new StreamReader(stream));
            var ostack = new Stack<Context>();

            Context currentContext = null;
            Func<Context, object, Context> currentParser = null;

            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonToken.StartObject:

                        if (currentContext == null)
                        {
                            currentContext = new Context(rootSubject, rootTerm);
                            currentParser = rootTerm.Parser;
                        }
                        else
                        {
                            // Save current context before creating new context
                            ostack.Push(currentContext);

                            if (currentParser != null) // If we have a handler to create a new context object
                            {
                                // Update to new Context unless we are walking an Array of objects
                                var newContext = currentParser(currentContext, currentContext.LastProperty);
                                if (newContext != null)  // Not an array
                                {
                                    currentContext = newContext;
                                }
                            }
                            else
                            {
                                currentContext = new Context(null, null);  // Unknown object
                            }
                        }
                     
                        DiagSource?.Write("Hapikit.JsonStreamingParser.NewObject", currentContext);
                        break;
                    case JsonToken.StartArray:
                        DiagSource?.Write("Hapikit.JsonStreamingParser.StartArray", currentContext);
                        break;
                    case JsonToken.EndArray:
                        DiagSource?.Write("Hapikit.JsonStreamingParser.EndArray", currentContext);
                        break;
                    case JsonToken.EndObject:
                        // Update context object
                        DiagSource?.Write("Hapikit.JsonStreamingParser.EndObject", currentContext);
                        if (ostack.Count > 0)
                        {
                            currentParser = currentContext?.TermMap?.Parser;
                            currentContext = ostack.Pop();
                        }
                        break;

                    case JsonToken.PropertyName:
                        // Determine new VocabTerm based on new Property
                        currentContext.LastProperty = reader.Value.ToString();
                        currentParser = currentContext.TermMap?.FindParser(currentContext.LastProperty);
                        DiagSource?.Write("Hapikit.JsonStreamingParser.PropertyName", currentContext.TermMap);
                        break;
                    case JsonToken.Integer:
                    case JsonToken.Boolean:
                    case JsonToken.String:
                        // Use VocabTerm Parser to Update Subject based on the current Term.
                        if (currentParser != null)
                        {
                            DiagSource?.Write("Hapikit.JsonStreamingParser.PropertyValue", reader.Value);
                            currentParser(currentContext, reader.Value);
                        }
                        else
                        {
                            DiagSource?.Write("Hapikit.JsonStreamingParser.MissingTermHandler", new { Context = currentContext, Value = reader.Value });
                        }
                        break;
                }
            }
        }


 

        private static Context StartObject(object rootSubject, VocabTerm rootTerm, Stack<Context> ostack, Context currentContext, VocabTerm currentTerm)
        {
            if (currentContext == null)
            {
                // Create context for Outer JSON Object
                currentContext = new Context(rootSubject, rootTerm);
            }
            else
            {
                // Save current context before creating new context
                ostack.Push(currentContext);

                if (currentTerm != null) // If we have a handler to create a new context object
                {
                    // Update to new Context unless we are walking an Array of objects
                    var newContext = currentTerm.Parser(currentContext, currentContext.TermMap);
                    if (newContext != null)  // Not an array
                    {
                        currentContext = newContext;
                    }
                }
                else
                {
                    currentContext = new Context(null, null);  // Unknown object
                }

            }

            return currentContext;
        }
    }

}
