using System;
using System.Collections.Generic;
using System.Text;

namespace Utils.NET.Collections
{
    public class ExpirationQueue<T>
    {
        private class ExpirationObject
        {
            public DateTime expirationTime;

            public T obj;

            public ExpirationObject(DateTime expirationTime, T obj)
            {
                this.expirationTime = expirationTime;
                this.obj = obj;
            }
        }

        /// <summary>
        /// All objects queued in their expiration order
        /// </summary>
        private Queue<ExpirationObject> queue = new Queue<ExpirationObject>();

        /// <summary>
        /// The time that each object lives for
        /// </summary>
        private double timeToLive;

        /// <summary>
        /// Creates a UniformExpirationQueue with a given expiration time in seconds
        /// </summary>
        /// <param name="timeToLive">The expiration time in seconds</param>
        public ExpirationQueue(double timeToLive)
        {
            this.timeToLive = timeToLive;
        }

        public void Enqueue(T obj)
        {
            queue.Enqueue(new ExpirationObject(DateTime.Now.AddSeconds(timeToLive), obj));
        }

        public IEnumerable<T> GetExpired()
        {
            var time = DateTime.Now;
            while (Peek(out var e))
            {
                if (time < e.expirationTime) yield break;
                e = queue.Dequeue();
                yield return e.obj;
            }
        }

        private bool Peek(out ExpirationObject obj)
        {
            if (queue.Count == 0)
            {
                obj = null;
                return false;
            }
            obj = queue.Peek();
            return true;
        }
    }
}
