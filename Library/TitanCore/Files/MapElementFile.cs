using System;
using System.IO;
using TitanCore.Core;
using Utils.NET.IO;

namespace TitanCore.Files
{
    public class MapElementFile
    {
        /// <summary>
        /// Reads a map element file from a given filepath
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public static MapElementFile ReadFrom(string filepath)
        {
            return ReadFrom(File.OpenRead(filepath));
        }

        /// <summary>
        /// Reads a map element file from a given stream
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static MapElementFile ReadFrom(Stream stream)
        {
            var file = new MapElementFile();
            file.Read(stream);
            return file;
        }

        public struct MapTileElement
        {
            /// <summary>
            /// The type of tile
            /// </summary>
            public ushort tileType;

            /// <summary>
            /// The type of object on the tile
            /// </summary>
            public ushort objectType;
        }

        public struct MapEntityElement
        {
            /// <summary>
            /// The x position of the entity
            /// </summary>
            public float x;

            /// <summary>
            /// The y position of the entity
            /// </summary>
            public float y;

            /// <summary>
            /// The type of entity
            /// </summary>
            public ushort entityType;
        }

        public struct MapRegionElement
        {
            /// <summary>
            /// The x position of the entity
            /// </summary>
            public uint x;

            /// <summary>
            /// The y position of the entity
            /// </summary>
            public uint y;

            /// <summary>
            /// The type of entity
            /// </summary>
            public Region regionType;
        }

        /// <summary>
        /// The width of the element
        /// </summary>
        public int width;

        /// <summary>
        /// The height of the element
        /// </summary>
        public int height;

        /// <summary>
        /// The tiles and objects within the map
        /// </summary>
        public MapTileElement[,] tiles;

        /// <summary>
        /// The entities within the map
        /// </summary>
        public MapEntityElement[] entities;

        /// <summary>
        /// The regions within the map
        /// </summary>
        public MapRegionElement[] regions;

        /// <summary>
        /// Reads the map file from a stream
        /// </summary>
        /// <param name="stream"></param>
        public void Read(Stream stream)
        {
            using (BinaryReader r = new BinaryReader(stream))
            {
                width = r.ReadInt32(); // read map size
                height = r.ReadInt32();

                tiles = new MapTileElement[width, height]; // read all tiles and objects
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        var t = new MapTileElement(); // read tile/object
                        t.tileType = r.ReadUInt16();
                        t.objectType = r.ReadUInt16();
                        tiles[x, y] = t; // set tile/object
                    }
                }

                entities = new MapEntityElement[r.ReadInt32()]; // create entity array with read length
                for (int i = 0; i < entities.Length; i++)
                {
                    var e = new MapEntityElement(); // read entity
                    e.x = r.ReadSingle();
                    e.y = r.ReadSingle();
                    e.entityType = r.ReadUInt16();
                    entities[i] = e; // set entity
                }

                regions = new MapRegionElement[r.ReadInt32()];
                for (int i = 0; i < regions.Length; i++)
                {
                    var e = new MapRegionElement(); // read region
                    e.x = r.ReadUInt32();
                    e.y = r.ReadUInt32();
                    e.regionType = (Region)r.ReadUInt16();
                    regions[i] = e; // set region
                }
            }
        }

        /// <summary>
        /// Writes the map file to a stream
        /// </summary>
        /// <param name="stream"></param>
        public void Write(Stream stream)
        {
            using (BinaryWriter w = new BinaryWriter(stream))
            {
                w.Write(width); // write map size
                w.Write(height);

                for (int y = 0; y < height; y++) // write all tiles and objects by looping the grid. Writes 0 for no tile or no object
                {
                    for (int x = 0; x < width; x++)
                    {
                        var t = tiles[x, y]; // write tile/object
                        w.Write(t.tileType);
                        w.Write(t.objectType);
                    }
                }

                w.Write(entities.Length); // write all entities
                for (int i = 0; i < entities.Length; i++)
                {
                    var e = entities[i]; // write entity
                    w.Write(e.x);
                    w.Write(e.y);
                    w.Write(e.entityType);
                }

                w.Write(regions.Length);
                for (int i = 0; i < regions.Length; i++)
                {
                    var e = regions[i]; // write region
                    w.Write(e.x);
                    w.Write(e.y);
                    w.Write((ushort)e.regionType);
                }
            }
        }
    }
}
