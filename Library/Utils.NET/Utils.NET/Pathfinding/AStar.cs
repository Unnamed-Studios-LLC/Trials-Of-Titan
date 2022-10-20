using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.Utils;

namespace Utils.NET.Pathfinding
{
    public static class AStar
    {
        private class AStarNode<T> where T : IPathNode<T>
        {
            public T pathNode;

            public float g = float.MaxValue;
            public float h;
            public float f;

            public AStarNode<T> parent;

            public AStarNode(T pathNode)
            {
                this.pathNode = pathNode;
            }
        }

        private static List<T> ConstructPath<T>(AStarNode<T> node) where T : IPathNode<T>
        {
            var list = new List<T>();
            while (node != null)
            {
                list.Add(node.pathNode);
                node = node.parent;
            }
            list.Reverse();
            return list;
        }

        public static List<T> Pathfind<T>(T start, T end) where T : IPathNode<T>
        {
            var dict = new Dictionary<T, AStarNode<T>>();

            var openList = new HashSet<AStarNode<T>>();

            var startNode = new AStarNode<T>(start);
            startNode.g = 0;
            dict.Add(start, startNode);
            openList.Add(startNode);

            while (openList.Count > 0)
            {
                var currentNode = openList.Min((a, b) => a.f < b.f ? a : b);
                if (currentNode.pathNode.Equals(end))
                {
                    return ConstructPath(currentNode);
                }
                openList.Remove(currentNode);

                foreach (var childPathNode in currentNode.pathNode.Adjacent)
                {
                    if (!dict.TryGetValue(childPathNode, out var child))
                    {
                        child = new AStarNode<T>(childPathNode);
                        dict.Add(childPathNode, child);
                    }

                    var tentativeG = currentNode.g + child.pathNode.Position.DistanceTo(currentNode.pathNode.Position);
                    if (tentativeG < child.g)
                    {
                        child.parent = currentNode;
                        child.g = tentativeG;
                        child.h = child.pathNode.Position.DistanceTo(end.Position);
                        child.f = child.g + child.h;

                        if (!openList.Contains(child))
                        {
                            openList.Add(child);
                        }
                    }
                }
            }
            return null;
        } 
    }
}
