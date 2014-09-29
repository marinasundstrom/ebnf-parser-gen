using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sundstrom.Ebnf
{
    public static class RuleExtensions
    {
        public static Expression First(this Alternation alternation)
        {
            return alternation.Left;
        }

        public static Expression First(this Concatenation alternation)
        {
            return alternation.Left;
        }

        public static IEnumerable<Expression> Concatenations(this Concatenation node)
        {
            yield return node.Left;

            var item = node.Right as Concatenation;
            if (item != null)
            {
                foreach (var alternation in Concatenations(item))
                {
                    yield return alternation;
                }
            }
            else
            {
                yield return item;
            }
        }

        public static IEnumerable<Expression> Alternations(this Alternation node)
        {
            yield return node.Left;

            var item = node.Right as Alternation;
            if (item != null)
            {
                foreach (var alternation in Alternations(item))
                {
                    yield return alternation;
                }
            }
            else
            {
                yield return item;
            }
        }
    }
}
