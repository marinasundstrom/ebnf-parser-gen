using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sundstrom.Ebnf
{
    public static class NonTerminalExtensions
    {
        public static IEnumerable<Expression> GetRuleProductions(this NonTerminal source)
        {
            var alternation = source.Rule as Alternation;
            if(alternation != null)
            {
                return alternation.Alternations();
            }
            return new[] { source.Rule };
        }

        public static bool IsLeftRecursive(this NonTerminal source)
        {
            var alternation = source.Rule as Alternation;
            if(alternation != null)
            {
                var alternations = alternation.Alternations().ToArray();
            }
          
            return false;
        }
    }
}
