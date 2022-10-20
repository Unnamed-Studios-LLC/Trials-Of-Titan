using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.Utils;

namespace Utils.NET.Collections
{
    public class CeilingRangeTable<T>
    {
        private Dictionary<float, T> values = new Dictionary<float, T>();
        
        public T this[float key]
        {
            get
            {
                return Get(key);
            }
            set
            {
                values[key] = value;
            }
        }

        public void Add(float key, T value)
        {
            values.Add(key, value);
        }

        public bool ContainsKey(float key)
        {
            return values.ContainsKey(key);
        }

        public T Get(float key)
        {
            return values.Closest(_ => _.Key < key ? float.MaxValue : _.Key - key).Value;
        }
    }
}
