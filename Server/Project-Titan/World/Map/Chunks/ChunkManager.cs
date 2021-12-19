using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.Geometry;

namespace World.Map.Chunks
{
    public class ChunkManager<T> where T : IChunkable
    {
        /// <summary>
        /// The width of the chunked map
        /// </summary>
        public int width;

        /// <summary>
        /// The height of the chunked map
        /// </summary>
        public int height;

        /// <summary>
        /// The size of each chunk
        /// </summary>
        public int chunkSize;

        /// <summary>
        /// The width of the map, in chunks
        /// </summary>
        public int chunkWidth;

        /// <summary>
        /// The height of the map, in chunks
        /// </summary>
        public int chunkHeight;

        /// <summary>
        /// Grid array of all chunks
        /// </summary>
        private Chunk<T>[,] chunks;

        public ChunkManager(int width, int height, int chunkSize)
        {
            this.width = width;
            this.height = height;
            this.chunkSize = chunkSize;

            CreateChunks();
        }

        /// <summary>
        /// Creates the chunk array and fills it with chunk instances
        /// </summary>
        private void CreateChunks()
        {
            chunkWidth = (width + chunkSize - 1) / chunkSize;
            chunkHeight = (height + chunkSize - 1) / chunkSize;

            chunks = new Chunk<T>[chunkWidth, chunkHeight];
            for (int y = 0; y < chunkHeight; y++)
            {
                for (int x = 0; x < chunkWidth; x++)
                {
                    chunks[x, y] = new Chunk<T>();
                }
            }
        }

        /// <summary>
        /// Adds an object to the chunk map
        /// </summary>
        /// <param name="obj"></param>
        public void AddObject(T obj)
        {
            Int2 pos = obj.IntPosition;
            obj.CurrentChunk = pos;

            int x = Math.Max(Math.Min(pos.x / chunkSize, chunkWidth), 0);
            int y = Math.Max(Math.Min(pos.y / chunkSize, chunkHeight), 0);

            chunks[x, y].Add(obj);
        }

        /// <summary>
        /// Removes an object from the chunk map
        /// </summary>
        /// <param name="obj"></param>
        public void RemoveObject(T obj)
        {
            Int2 pos = obj.CurrentChunk;

            int x = Math.Max(Math.Min(pos.x / chunkSize, chunkWidth), 0);
            int y = Math.Max(Math.Min(pos.y / chunkSize, chunkHeight), 0);

            chunks[x, y].Remove(obj);
        }

        /// <summary>
        /// Updates the object's chunk, if changed
        /// </summary>
        /// <param name="obj"></param>
        public void UpdateObject(T obj)
        {
            Int2 last = obj.CurrentChunk;
            Int2 cur = obj.IntPosition;
            obj.CurrentChunk = cur;

            int x = Math.Max(Math.Min(cur.x / chunkSize, chunkWidth), 0);
            int y = Math.Max(Math.Min(cur.y / chunkSize, chunkHeight), 0);
            int lastX = Math.Max(Math.Min(last.x / chunkSize, chunkWidth), 0);
            int lastY = Math.Max(Math.Min(last.y / chunkSize, chunkHeight), 0);

            if (x == lastX && y == lastY) return;
            chunks[lastX, lastY].Remove(obj);
            chunks[x, y].Add(obj);
        }

        /// <summary>
        /// Returns all objects within a distance from a point
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public IEnumerable<T> GetWithin(float x, float y, float distance, Func<T, bool> where)
        {
            Vec2 pos = new Vec2(x, y);
            float sqrDistance = distance * distance;

            int lx = (int)(x - distance); // check bounding points
            int ly = (int)(y - distance);
            int hx = (int)(x + distance);
            int hy = (int)(y + distance);

            lx = Math.Max(Math.Min(lx / chunkSize, chunkWidth), 0); // bounds check
            ly = Math.Max(Math.Min(ly / chunkSize, chunkHeight), 0);
            hx = Math.Max(Math.Min(hx / chunkSize, chunkWidth), 0);
            hy = Math.Max(Math.Min(hy / chunkSize, chunkHeight), 0);

            for (int cy = ly; cy <= hy; cy++) // loop chunks and return chunk objects
            {
                for (int cx = lx; cx <= hx; cx++)
                {
                    var chunk = chunks[cx, cy];
                    if (!chunk.HasObjects) continue;
                    foreach (var obj in chunks[cx, cy].GetObjects())
                    {
                        if ((obj.Position - pos).SqrLength > sqrDistance || (!where?.Invoke(obj) ?? true)) continue;
                        yield return obj;
                    }
                }
            }
        }
    }
}
