using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Utils.NET.Collections
{
    public class ConcurrentExpirationQueue<T>
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
        private ConcurrentQueue<ExpirationObject> queue = new ConcurrentQueue<ExpirationObject>();

        /// <summary>
        /// The time that each object lives for
        /// </summary>
        private double timeToLive;

        /// <summary>
        /// Creates a UniformExpirationQueue with a given expiration time in seconds
        /// </summary>
        /// <param name="timeToLive">The expiration time in seconds</param>
        public ConcurrentExpirationQueue(double timeToLive)
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
            while (queue.TryPeek(out var e))
            {
                if (time < e.expirationTime) yield break;
                queue.TryDequeue(out e);
                yield return e.obj;
            }
        }
    }
}
