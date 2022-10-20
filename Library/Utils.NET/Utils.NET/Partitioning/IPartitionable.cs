using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.Geometry;

namespace Utils.NET.Partitioning
{
    public interface IPartitionable
    {
        Rect BoundingRect { get; }

        IntRect LastBoundingRect { get; set; }
    }
}
