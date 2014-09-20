using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sundstrom
{
    public static class EnumerableExtensions
    {
        public static Queue<TSource> ToQueue<TSource>(this IEnumerable<TSource> source)
        {
            return new Queue<TSource>(source);
        }

        public static Queue<TSource> ToReverseQueue<TSource>(this IEnumerable<TSource> source)
        {
            return new Queue<TSource>(source.Reverse());
        }
    }
}
