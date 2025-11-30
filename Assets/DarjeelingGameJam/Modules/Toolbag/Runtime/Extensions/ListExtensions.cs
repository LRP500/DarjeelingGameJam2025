using System;
using System.Collections.Generic;
using System.Linq;

namespace Modules.Toolbag.Extensions
{
    public static class ListExtensions
    {
        public static bool IsNullOrEmpty<T>(this IList<T> list)
        {
            return list == null || list.Count == 0;
        }
        
        public static T Random<T>(this IList<T> list)
        {
            return list.IsNullOrEmpty() ? default : list[UnityEngine.Random.Range(0, list.Count)];
        }
        
        public static IList<T> Swap<T>(this IList<T> list, int indexA, int indexB)
        {
            (list[indexA], list[indexB]) = (list[indexB], list[indexA]);
            return list;
        }
        
        public static void Shuffle<T>(this IList<T> list)  
        {  
            var n = list.Count;
            var rng = new Random();  

            while (n > 1)
            {
                n--;  
                var k = rng.Next(n + 1);  
                (list[k], list[n]) = (list[n], list[k]);
            }
        }
        
        public static T RemoveLast<T>(this List<T> source)
        {
            if (!source.Any())
            {
                return default;
            }

            var item = source.Last();

            source.RemoveAt(source.Count - 1);

            return item;
        }
    }
}