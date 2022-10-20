using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.Geometry;
using Utils.NET.Logging;

namespace Utils.NET.Partitioning
{
    public abstract class PartitionMap<T> where T : IPartitionable
    {
        /// <summary>
        /// The size of partitioned sections
        /// </summary>
        protected int partitionSize;

        public PartitionMap(int partitionSize)
        {
            this.partitionSize = partitionSize;
        }

        public IEnumerable<T> GetNearObjects(Rect rect)
        {
            foreach (var point in GetPartitionPoints(rect.GetIntRect()))
            {
                foreach (var obj in GetPartition(point.x, point.y))
                    yield return obj;
            }
        }

        /// <summary>
        /// Adds an object to the map
        /// </summary>
        /// <param name="o"></param>
        public void Add(T o)
        {
            var rect = o.BoundingRect.GetIntRect();
            o.LastBoundingRect = rect;
            foreach (var point in GetPartitionPoints(o.LastBoundingRect))
            {
                AddToPartition(o, point.x, point.y);
            }
        }

        /// <summary>
        /// Removes an object from the map
        /// </summary>
        /// <param name="o"></param>
        public void Remove(T o)
        {
            foreach (var point in GetPartitionPoints(o.LastBoundingRect))
            {
                RemoveFromPartition(o, point.x, point.y);
            }
        }

        /// <summary>
        /// Updates the position of an object
        /// </summary>
        /// <param name="o"></param>
        public void Update(T o)
        {
            var newBounding = o.BoundingRect.GetIntRect();
            if (newBounding == o.LastBoundingRect) return;
            foreach (var point in GetPartitionPoints(o.LastBoundingRect))
            {
                RemoveFromPartition(o, point.x, point.y);
            }
            foreach (var point in GetPartitionPoints(newBounding))
            {
                AddToPartition(o, point.x, point.y);
            }
            o.LastBoundingRect = newBounding;
        }

        /// <summary>
        /// Returns all points to partitions that the given rect is in
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        protected virtual IEnumerable<Int2> GetPartitionPoints(IntRect rect)
        {
            Int2 bl = rect.BottomLeft / partitionSize;
            Int2 tr = rect.TopRight / partitionSize;

            for (int y = bl.y; y <= tr.y; y++)
            {
                for (int x = bl.x; x <= tr.x; x++)
                {
                    yield return new Int2(x, y);
                }
            }
        }

        public IEnumerable<T> GetPartitionFromPoint(int x, int y)
        {
            return GetPartition(x / partitionSize, y / partitionSize);
        }

        /// <summary>
        /// Gets the partition at a given partition point
        /// </summary>
        /// <param name="partitionX"></param>
        /// <param name="partitionY"></param>
        /// <returns></returns>
        protected abstract IEnumerable<T> GetPartition(int partitionX, int partitionY);

        /// <summary>
        /// Adds an object to a partition
        /// </summary>
        /// <param name="o"></param>
        /// <param name="partitionX"></param>
        /// <param name="partitionY"></param>
        protected abstract void AddToPartition(T o, int partitionX, int partitionY);

        /// <summary>
        /// Removes an object from a partition
        /// </summary>
        /// <param name="o"></param>
        /// <param name="partitionX"></param>
        /// <param name="partitionY"></param>
        protected abstract void RemoveFromPartition(T o, int partitionX, int partitionY);
    }
}
