using System;
using System.Collections.Generic;

namespace Hapikit
{
    public class VocabTerm
    {
        protected Func<Context, object, Context> _DefaultChildParser;

        private Dictionary<string, VocabTerm> _ChildTerms = new Dictionary<string, VocabTerm>();

        public VocabTerm() : this(null)
        {

        }
        public VocabTerm(string term)
        {
            Term = term;
        }

        public string Term
        {
            get; private set;
        } 
        
        public Func<Context, object, Context> Parser
        {
            get; set;
        } 

        public VocabTerm Find(string term)
        {
            if (_ChildTerms.ContainsKey(term))
            {
                return _ChildTerms[term];
            } else
            {
                return null;
            }
        }


        public void AddChild(string term, VocabTerm childTerm)
        {
            _ChildTerms.Add(term, childTerm);
        }
        
        internal Func<Context, object, Context> FindParser(string propertyName)
        {
            var term = Find(propertyName);
            if (term != null )
            {
                return term.Parser;
            } else
            {
                return _DefaultChildParser;
            }
        }

        public VocabTerm<NS> Clone<NS>(string newterm)
        {
            var term = new VocabTerm<NS>(newterm);
            foreach(var ct in _ChildTerms)
            {
                term.AddChild(ct.Key, ct.Value);
            }
            return term;
        }

    }

    public class VocabTerm<S> : VocabTerm
    {
        public VocabTerm(Action<VocabTerm<S>> configure = null) : this(null, configure) {
        }

        public VocabTerm(string term, Action<VocabTerm<S>> configure = null) : base(term)
        {
            configure?.Invoke(this);
        }

        public void MapProperty<T>(string term, Action<S, T> parser)
        {
            var newterm = new VocabTerm(term);
            newterm.Parser = (c,o) => {
                    parser((S)c.Subject, (T)Convert.ChangeType(o,typeof(T)));
                return null;
            };
            AddChild(term, newterm);
        }

        public void MapObject<T>(VocabTerm term, Func<S, T> parser)
        {
            
            term.Parser = (c, o) => {
                return new Context(parser((S)c.Subject), term); 
            };
            AddChild(term.Term, term);
        }
        public void MapObject<T>(string alternateTerm, VocabTerm term, Func<S, T> parser)
        {

            term.Parser = (c, o) => {
                return new Context(parser((S)c.Subject), term);
            };
            AddChild(alternateTerm, term);
        }

        public void MapAnyObject<T>(VocabTerm term, Func<S,string, T> parser)
        {

            _DefaultChildParser = (c, o) => {
                return new Context(parser((S)c.Subject,(string)o), term);
            };
         
        }
    }

  
}