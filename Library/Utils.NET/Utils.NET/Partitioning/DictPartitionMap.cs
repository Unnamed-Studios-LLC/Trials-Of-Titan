using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.Geometry;

namespace Utils.NET.Partitioning
{
    public class DictPartitionMap<T> : PartitionMap<T> where T : IPartitionable
    {
        /// <summary>
        /// Dictionary containing partitions
        /// </summary>
        private Dictionary<ulong, List<T>> partitions = new Dictionary<ulong, List<T>>();

        /// <summary>
        /// Empty list returned if no partition exists in a requested area
        /// </summary>
        private List<T> empty = new List<T>();

        public DictPartitionMap(int partitionSize) : base(partitionSize)
        {
        }

        protected override IEnumerable<T> GetPartition(int partitionX, int partitionY)
        {
            var key = ((ulong)partitionX << 32) | ((uint)partitionY);
            if (!partitions.TryGetValue(key, out var partition))
                return empty;
            return partition;
        }

        protected override void AddToPartition(T o, int partitionX, int partitionY)
        {
            var key = ((ulong)partitionX << 32) | ((uint)partitionY);
            if (!partitions.TryGetValue(key, out var partition))
            {
                partition = new List<T>();
                partitions.Add(key, partition);
            }
            partition.Add(o);
        }

        protected override void RemoveFromPartition(T o, int partitionX, int partitionY)
        {
            var key = ((ulong)partitionX << 32) | ((uint)partitionY);
            var partition = partitions[key];
            partition.Remove(o);
            if (partition.Count == 0)
                partitions.Remove(key);
        }
    }
}
