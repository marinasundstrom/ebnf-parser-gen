using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sundstrom.Ebnf
{
    public static class ExpressionExtensions
    {
        public static Option Option(this Expression node)
        {
            return new Option(node);
        }

        public static Repetition Repeat(this Expression node)
        {
            return new Repetition(node);
        }

        public static IEnumerable<Expression> AsEnumerable(this Expression node)
        {
            var concatenation = node as Concatenation;
            if (concatenation != null)
            {
                if (concatenation.Left is Concatenation)
                {
                    foreach (var exp in concatenation.Left.AsEnumerable())
                    {
                        yield return exp;
                    }
                }
                else
                {
                    yield return concatenation.Left;
                }

                if (concatenation.Right is Concatenation)
                {
                    foreach (var exp in concatenation.Right.AsEnumerable())
                    {
                        yield return exp;
                    }
                }
                else
                {
                    yield return concatenation.Right;
                }

            }
            else
            {
                yield return node;
            }
        }
    }
}
