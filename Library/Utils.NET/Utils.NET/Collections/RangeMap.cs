using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.Utils;

namespace Utils.NET.Collections
{
    public struct Range
    {
        /// <summary>
        /// The minimum value of this range
        /// </summary>
        public float min;

        /// <summary>
        /// The maximum value of this range
        /// </summary>
        public float max;

        public Range(float min, float max)
        {
            this.min = min;
            this.max = max;
        }

        /// <summary>
        /// Returns a random value within the min/max range
        /// </summary>
        /// <returns></returns>
        public float GetRandom()
        {
            if (min == max) return min;
            return min + (max - min) * Rand.FloatValue();
        }

        public float Average() => min + (max - min) / 2;

        public static implicit operator Range(float value) => new Range(value, value);
    }

    public class RangePair<T>
    {
        public Range range;
        public T value;

        public RangePair(Range range, T value)
        {
            this.range = range;
            this.value = value;
        }

        public bool InRange(float value)
        {
            return value >= range.min && value < range.max;
        }
    }

    public class RangeMap<T>
    {
        public int Count => rangePairs.Count;

        private List<RangePair<T>> rangePairs = new List<RangePair<T>>();

        public RangeMap()
        {

        }

        public RangeMap(IEnumerable<RangePair<T>> pairs)
        {
            rangePairs.AddRange(pairs);
        }

        public T this[float i]
        {
            get => Get(i);
        }

        public T Get(float index)
        {
            foreach (var pair in rangePairs)
            {
                if (pair.InRange(index))
                    return pair.value;
            }
            return default;
        }

        public void Add(Range range, T value)
        {
            rangePairs.Add(new RangePair<T>(range, value));
        }

        public void Add(RangePair<T> pair)
        {
            rangePairs.Add(pair);
        }
    }
}
