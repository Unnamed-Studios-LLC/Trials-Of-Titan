using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.Geometry;
using Utils.NET.Logging;

namespace Utils.NET.Partitioning
{
    public class ArrayPartitionMap<T> : PartitionMap<T> where T : IPartitionable
    {
        /// <summary>
        /// The width of the map
        /// </summary>
        private int width;

        /// <summary>
        /// The height of the map
        /// </summary>
        private int height;

        /// <summary>
        /// Grid array of all partitions
        /// </summary>
        private HashSet<T>[,] partitions;

        private Int2 min;
        private Int2 max;

        public ArrayPartitionMap(int width, int height, int partitionSize) : base(partitionSize)
        {
            this.width = width;
            this.height = height;

            partitions = new HashSet<T>[(width + partitionSize - 1) / partitionSize, (height + partitionSize - 1) / partitionSize];
            for (int y = 0; y < partitions.GetLength(1); y++)
            {
                for (int x = 0; x < partitions.GetLength(0); x++)
                {
                    partitions[x, y] = new HashSet<T>();
                }
            }

            min = new Int2(0, 0);
            max = new Int2(partitions.GetLength(0) - 1, partitions.GetLength(1) - 1);
        }

        protected override IEnumerable<Int2> GetPartitionPoints(IntRect rect)
        {
            Int2 bl = rect.BottomLeft / partitionSize;
            Int2 tr = rect.TopRight / partitionSize;

            bl = bl.Clamp(min, max);
            tr = tr.Clamp(min, max);

            for (int y = bl.y; y <= tr.y; y++)
            {
                for (int x = bl.x; x <= tr.x; x++)
                {
                    yield return new Int2(x, y);
                }
            }
        }

        protected override IEnumerable<T> GetPartition(int partitionX, int partitionY)
        {
            return partitions[partitionX, partitionY];
        }

        protected override void AddToPartition(T o, int partitionX, int partitionY)
        {
            var partition = partitions[partitionX, partitionY];
            partition.Add(o);
        }

        protected override void RemoveFromPartition(T o, int partitionX, int partitionY)
        {
            var partition = partitions[partitionX, partitionY];
            partition.Remove(o);
        }
    }
}
