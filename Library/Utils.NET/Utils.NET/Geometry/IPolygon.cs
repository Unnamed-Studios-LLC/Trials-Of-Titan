using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.Partitioning;

namespace Utils.NET.Geometry
{
    public interface IPolygon : IPartitionable
    {
        Vec2[] GetPoints();

        Line[] GetEdges();

        bool Contains(Vec2 point);
    }
}
