using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.Geometry;

namespace World.Map.Chunks
{
    public interface IChunkable
    {
        Int2 IntPosition { get; }

        Vec2 Position { get; }

        Int2 CurrentChunk { get; set; }
    }

    public class Chunk<T> where T : IChunkable
    {
        public bool HasObjects => objects.Count != 0;

        private List<T> objects = new List<T>();

        public IEnumerable<T> GetObjects()
        {
            for (int i = 0; i < objects.Count; i++)
                yield return objects[i];
        }

        public void Add(T obj)
        {
            objects.Add(obj);
        }

        public void Remove(T obj)
        {
            objects.Remove(obj);
        }
    }
}
