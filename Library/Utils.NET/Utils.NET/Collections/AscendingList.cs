using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Utils.NET.Collections
{
    public interface IAscendingValue
    {
        int Value { get; }
    }

    public class AscendingList<T> : IList<T>  where T : IAscendingValue
    {
        private List<T> list = new List<T>();

        public T this[int index] { get => list[index]; set => list[index] = value; }

        public int Count => list.Count;

        public bool IsReadOnly => false;

        public void Add(T value)
        {
            Insert(0, value);
        }

        public void Clear()
        {
            list.Clear();
        }

        public bool Contains(T item)
        {
            return list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            list.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return list.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            var value = item.Value;
            for (int i = 0; i < list.Count; i++)
            {
                var v = list[i];
                if (value <= v.Value)
                {
                    list.Insert(i, item);
                }
            }
            list.Add(item);
        }

        public void Remove(T value)
        {
            list.Remove(value);
        }

        public void RemoveAt(int index)
        {
            list.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        bool ICollection<T>.Remove(T item)
        {
            return list.Remove(item);
        }
    }
}
