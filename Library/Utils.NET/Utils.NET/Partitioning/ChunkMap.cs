using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.Geometry;

namespace Utils.NET.Partitioning
{
    public interface IChunk
    {
        void UpdatePosition(Int2 position);

        void LoadChunk(IntRect bounds);

        void Dispose();
    }

    public class ChunkMap<T> where T : IChunk
    {
        private class Chunk
        {
            private bool first = true;

            public Int2 position;

            private Int2 focusPosition;

            public T obj;

            public Chunk(Int2 position, T obj)
            {
                this.position = position;
                this.obj = obj;
            }

            public void UpdateFocus(int chunkX, int chunkY, Int2 chunkViewportSize, int chunkSize)
            {
                int x = position.x, y = position.y;

                int xDif = chunkX - position.x;
                int yDif = chunkY - position.y;

                Int2 viewDif = new Int2(xDif % chunkViewportSize.x, yDif % chunkViewportSize.y);
                Int2 halfViewport = chunkViewportSize / 2;

                if (xDif > halfViewport.x)
                {
                    if (viewDif.x > halfViewport.x)
                    {
                        x = chunkX + (chunkViewportSize.x - viewDif.x);
                    }
                    else
                    {
                        x = chunkX - viewDif.x;
                    }
                }
                else if (xDif < -halfViewport.x)
                {
                    if (viewDif.x < -halfViewport.x)
                    {
                        x = chunkX - (chunkViewportSize.x + viewDif.x);
                    }
                    else
                    {
                        x = chunkX - viewDif.x;
                    }
                }

                if (yDif > halfViewport.y)
                {
                    if (viewDif.y > halfViewport.y)
                    {
                        y = chunkY + (chunkViewportSize.y - viewDif.y);
                    }
                    else
                    {
                        y = chunkY - viewDif.y;
                    }
                }
                else if (yDif < -halfViewport.y)
                {
                    if (viewDif.y < -halfViewport.y)
                    {
                        y = chunkY - (chunkViewportSize.y + viewDif.y);
                    }
                    else
                    {
                        y = chunkY - viewDif.y;
                    }
                }

                focusPosition = new Int2(x, y);
                obj.UpdatePosition(focusPosition);
            }

            public void LoadChunk(int chunkSize)
            {
                if (focusPosition == position && !first) return;
                first = false;
                position = focusPosition;
                obj.LoadChunk(new IntRect(position, new Int2(chunkSize, chunkSize)));
            }

            public void Dispose()
            {
                obj.Dispose();
            }
        }

        public int width;

        public int height;

        public int chunkSize;

        private Chunk[,] chunks;

        private Int2 chunkViewport;

        private Int2 focus = new Int2(int.MinValue, int.MinValue);

        public ChunkMap(int width, int height, int chunkSize, Int2 viewportSize, Func<Int2, int, T> createChunk)
        {
            this.width = width;
            this.height = height;
            this.chunkSize = chunkSize;

            chunkViewport = new Int2((viewportSize.x + chunkSize - 1) / chunkSize, (viewportSize.y + chunkSize - 1) / chunkSize); // ceiling division to get min chunks needed to cover the viewport
            chunks = new Chunk[chunkViewport.x, chunkViewport.y];

            for (int y = 0; y < chunkViewport.y; y++)
                for (int x = 0; x < chunkViewport.x; x++)
                {
                    var position = new Int2(x, y) * chunkSize;
                    chunks[x, y] = new Chunk(position, createChunk(position, chunkSize));
                }
        }

        public void SetFocus(int x, int y) => SetFocus(new Int2(x, y));

        public void SetFocus(Int2 newFocus)
        {
            if (focus.x == newFocus.x && focus.y == newFocus.y) return;

            int chunkX = (newFocus.x / chunkSize) * chunkSize;
            int chunkY = (newFocus.y / chunkSize) * chunkSize;

            focus = newFocus;
            for (int y = 0; y < chunkViewport.y; y++)
                for (int x = 0; x < chunkViewport.x; x++)
                {
                    chunks[x, y].UpdateFocus(chunkX, chunkY, chunkViewport * chunkSize, chunkSize);
                }

            for (int y = 0; y < chunkViewport.y; y++)
                for (int x = 0; x < chunkViewport.x; x++)
                {
                    chunks[x, y].LoadChunk(chunkSize);
                }
        }

        public T LiveChunkAt(int x, int y)
        {
            if (focus.x == int.MinValue) return default;
            int viewportX = x % (chunkViewport.x * chunkSize);
            int viewportY = y % (chunkViewport.y * chunkSize);

            int chunkX = viewportX / chunkSize;
            int chunkY = viewportY / chunkSize;

            var chunk = chunks[chunkX, chunkY];
            if (chunk.position != new Int2((x / chunkSize) * chunkSize, (y / chunkSize) * chunkSize))
                return default;
            return chunk.obj;
        }

        public void Dispose()
        {
            for (int y = 0; y < chunkViewport.y; y++)
                for (int x = 0; x < chunkViewport.x; x++)
                {
                    chunks[x, y].Dispose();
                }
        }
    }
}
