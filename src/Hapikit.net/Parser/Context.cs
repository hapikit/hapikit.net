using System;
using System.Linq;
using System.Collections.Generic;
namespace Hapikit
{
    public class Context
    {
        public object Subject { get; private set; }
        public VocabTerm TermMap { get; private set; }
        public string LastProperty { get; set; }

        public Context(object subject, VocabTerm term)
        {
            Subject = subject;
            TermMap = term;
        }

        public override string ToString()
        {
            return $"{Subject?.GetType().Name} :  {TermMap?.Term}";
        }
    }
}