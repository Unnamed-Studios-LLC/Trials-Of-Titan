using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utils.NET.Utils
{
    public static class ArrayUtils
    {

        /// <summary>
        /// Returns a random array element using Rand.Next
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <returns></returns>
        public static T Random<T>(this T[] array)
        {
            if (array.Length == 0) return default;
            return array[Rand.Next(array.Length)];
        }

        /// <summary>
        /// Randomizes the element order of this array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <returns></returns>
        public static T[] Randomize<T>(this T[] array)
        {
            var list = array.ToList();
            var newArray = new T[array.Length];
            int index = 0;
            while (list.Count > 0)
            {
                var rnd = Rand.Next(list.Count);
                newArray[index++] = list[rnd];
                list.RemoveAt(rnd);
            }
            return newArray;
        }
    }
}
