using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.Geometry;

namespace Utils.NET.Pathfinding
{
    public interface IPathNode<T> where T : IPathNode<T>
    {
        Vec2 Position { get; }

        IEnumerable<T> Adjacent { get; }
    }
}
