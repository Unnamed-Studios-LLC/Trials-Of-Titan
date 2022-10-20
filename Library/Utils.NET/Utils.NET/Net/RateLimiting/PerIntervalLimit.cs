using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using Utils.NET.Collections;

namespace Utils.NET.Net.RateLimiting
{
    /// <summary>
    /// A rate limiting class used to limit requests to a certain amount per a given interval.
    /// </summary>
    public class PerIntervalLimit
    {
        /// <summary>
        /// Request data for a specific IP address
        /// </summary>
        private class LimitedInstance
        {
            /// <summary>
            /// The address of the requester
            /// </summary>
            public IPAddress address;

            /// <summary>
            /// The amount of times a request is received
            /// </summary>
            public int count;

            public LimitedInstance(IPAddress address)
            {
                this.address = address;
                count = 0;
            }
        }

        /// <summary>
        /// The expiration queue for requests
        /// </summary>
        private ConcurrentExpirationQueue<LimitedInstance> expirationQueue;

        /// <summary>
        /// All requests
        /// </summary>
        private ConcurrentDictionary<IPAddress, LimitedInstance> instances = new ConcurrentDictionary<IPAddress, LimitedInstance>();

        /// <summary>
        /// The rate limit to use
        /// </summary>
        private readonly int rateLimit;

        public PerIntervalLimit(int rateLimit, double interval)
        {
            this.rateLimit = rateLimit;
            expirationQueue = new ConcurrentExpirationQueue<LimitedInstance>(interval);
        }

        /// <summary>
        /// Flushes expired request instances
        /// </summary>
        private void FlushExpired()
        {
            foreach (var expired in expirationQueue.GetExpired())
            {
                instances.TryRemove(expired.address, out var dummy);
            }
        }

        /// <summary>
        /// Returns if an address has exceeded the set rate limit, does NOT increment the rate limit
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public bool CanRequestNoTrigger(IPAddress address)
        {
            FlushExpired();

            if (!instances.TryGetValue(address, out var instance))
            {
                instance = new LimitedInstance(address);
                if (!instances.TryAdd(address, instance))
                {
                    if (!instances.TryGetValue(address, out instance))
                        return false;
                }
                else
                {
                    expirationQueue.Enqueue(instance);
                }
            }

            lock (instance)
            {
                return instance.count < rateLimit;
            }
        }

        /// <summary>
        /// Returns if an address has exceeded the set rate limit and increments the rate limit
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public bool CanRequest(IPAddress address)
        {
            FlushExpired();

            if (!instances.TryGetValue(address, out var instance))
            {
                instance = new LimitedInstance(address);
                if (!instances.TryAdd(address, instance))
                {
                    if (!instances.TryGetValue(address, out instance))
                        return false;
                }
                else
                {
                    expirationQueue.Enqueue(instance);
                }
            }

            lock (instance)
            {
                instance.count++;
                return instance.count <= rateLimit;
            }
        }

        public void Trigger(IPAddress address)
        {
            if (!instances.TryGetValue(address, out var instance))
            {
                instance = new LimitedInstance(address);
                if (!instances.TryAdd(address, instance))
                {
                    if (!instances.TryGetValue(address, out instance))
                        return;
                }
                else
                {
                    expirationQueue.Enqueue(instance);
                }
            }

            lock (instance)
            {
                instance.count++;
            }
        }
    }
}
