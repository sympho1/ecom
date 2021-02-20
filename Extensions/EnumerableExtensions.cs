using System;
using System.Collections.Generic;

namespace RestApi.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> Times<T>(this int count, Func<int, T> fn)
        {
            for (var i = 1; i <= count; i++) yield return fn.Invoke(i);
        }
        
    }
}