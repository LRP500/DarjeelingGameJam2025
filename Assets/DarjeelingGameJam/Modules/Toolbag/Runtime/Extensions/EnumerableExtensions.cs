using System.Collections.Generic;
using System.Linq;

namespace Modules.Toolbag.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            var items = source.ToList();
            ListExtensions.Shuffle(items);
            return items;
        }
    }
}