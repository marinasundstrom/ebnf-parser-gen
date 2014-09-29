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
    }
}
