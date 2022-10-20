using System;
using System.Collections.Generic;
using System.Text;

namespace Utils.NET.Utils
{
    public static class LinqEx
    {
        public static T Min<T>(this IEnumerable<T> collection, Func<T, T, T> comparison)
        {
            T result = default(T);
            foreach (var value in collection)
            {
                if (result == null)
                {
                    result = value;
                    continue;
                }

                result = comparison(result, value);
            }
            return result;
        }

        public static T Closest<T>(this IEnumerable<T> collection, Func<T, float> distance)
        {
            T result = default(T);
            float closestDistance = float.MaxValue;
            foreach (var value in collection)
            {
                var dis = distance(value);
                if (dis < closestDistance)
                {
                    result = value;
                    closestDistance = dis;
                }
            }
            return result;
        }

        public static IEnumerable<TResult> SelectWhere<T, TResult>(this IEnumerable<T> collection, Func<T, bool> where, Func<T, TResult> select)
        {
            foreach (var value in collection)
            {
                if (!where(value)) continue;
                yield return select(value);
            }
        }

        public static IEnumerable<TResult> WhereType<TResult>(this IEnumerable<object> collection) where TResult : class
        {
            foreach (var value in collection)
            {
                var asValue = value as TResult;
                if (asValue == null) continue;
                yield return asValue;
            }
        }
    }
}
