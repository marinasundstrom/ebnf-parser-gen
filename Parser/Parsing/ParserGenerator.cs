using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sundstrom.Ebnf;

namespace Sundstrom.Parsing
{
    /// <summary>
    /// Description of ParserGenerator.
    /// </summary>
    public sealed class ParserGenerator
    {
        public ParserGenerator(Grammar grammar)
        {
            Grammar = grammar;
        }

        public Grammar Grammar { get; private set; }

        public object Generate()
        {
            
            return null;
        }
    }
}
