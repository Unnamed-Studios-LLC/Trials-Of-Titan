using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Utils.NET.Collections;
using Utils.NET.Utils;

public enum BiomeType
{
    Ocean,
    BeachSpawn,
    Beach,
    Grasslands,
    Wetlands,
    Desert,
    Lava,
    Snow,
    Lake
}

public class Biome
{
    private static Biome[] biomes;

    static Biome()
    {
        biomes = new Biome[Enum.GetValues(typeof(BiomeType)).Length];
        for (int i = 0; i < biomes.Length; i++)
        {
            var biomeType = (BiomeType)i;
            biomes[i] = CreateBiome(biomeType);
        }
    }

    private static Biome CreateBiome(BiomeType type)
    {
        switch (type)
        {
            case BiomeType.Desert:
                return new Biome()
                {
                    type = type,
                    displayColor = new Color(146f / 255f, 114f / 255f, 74f / 255f, 1),
                    perlinScale = 100,
                    tiles = new RangeMap<ushort>(new RangePair<ushort>[]
                    {
                        new RangePair<ushort>(new Range(float.MinValue, 0.5f), 0xb07),
                        new RangePair<ushort>(new Range(0.5f, 0.88f), 0xb29),
                        new RangePair<ushort>(new Range(0.88f, float.MaxValue), 0xb08)
                    }),
                    objects = new RangeMap<BiomeObject>(new RangePair<BiomeObject>[]
                    {
                        new RangePair<BiomeObject>(new Range(float.MinValue, 0.01f), new BiomeObject(0xb07, 0xa05)),
                        new RangePair<BiomeObject>(new Range(0.01f, 0.015f), new BiomeObject(0xb07, 0xa06)),
                        new RangePair<BiomeObject>(new Range(0.985f, 0.99f), new BiomeObject(0xb29, 0xa06)),
                        new RangePair<BiomeObject>(new Range(0.99f, float.MaxValue), new BiomeObject(0xb29, 0xa05))
                    })
                };
            case BiomeType.Lava:
                return new Biome()
                {
                    type = type,
                    displayColor = new Color(41f / 255f, 42f / 255f, 43f / 255f, 1),
                    perlinScale = 100,
                    tiles = new RangeMap<ushort>(new RangePair<ushort>[]
                    {
                        new RangePair<ushort>(new Range(float.MinValue, 0.88f), 0xb0d),
                        new RangePair<ushort>(new Range(0.88f, float.MaxValue), 0xb0e)
                    }),
                    objects = new RangeMap<BiomeObject>(new RangePair<BiomeObject>[]
                    {
                        new RangePair<BiomeObject>(new Range(0.985f, float.MaxValue), new BiomeObject(0xb0d, 0xa25)),
                    })
                };
            case BiomeType.Snow:
                return new Biome()
                {
                    type = type,
                    displayColor = new Color(176f / 255f, 202f / 255f, 212f / 255f, 1),
                    perlinScale = 120,
                    tiles = new RangeMap<ushort>(new RangePair<ushort>[]
                    {
                        new RangePair<ushort>(new Range(float.MinValue, 0.22f), 0xb21), // frozen lake
                        new RangePair<ushort>(new Range(0.22f, 0.8f), 0xb1e), // full snow
                        new RangePair<ushort>(new Range(0.8f, 0.88f), 0xb1f), // partial snow
                        new RangePair<ushort>(new Range(0.88f, float.MaxValue), 0xb20) // no snow
                    }),
                    objects = new RangeMap<BiomeObject>(new RangePair<BiomeObject>[]
                    {
                        new RangePair<BiomeObject>(new Range(0.59f, 0.6f), new BiomeObject(0xb1e, 0xa3e, 0xa3f, 0xa40, 0xa41)),
                        new RangePair<BiomeObject>(new Range(0.6f, float.MaxValue), new BiomeObject(0xb20, 0xa3a, 0xa3b, 0xa3c, 0xa3d)),
                    })
                };
            case BiomeType.Wetlands:
                return new Biome()
                {
                    type = type,
                    displayColor = new Color(32f / 255f, 78f / 255f, 69f / 255f, 1),
                    perlinScale = 300,
                    tiles = new RangeMap<ushort>(new RangePair<ushort>[]
                    {
                        new RangePair<ushort>(new Range(float.MinValue, 0.7f), 0xb05),
                        new RangePair<ushort>(new Range(0.7f, float.MaxValue), 0xb06)
                    }),
                    objects = new RangeMap<BiomeObject>(new RangePair<BiomeObject>[]
                    {
                        new RangePair<BiomeObject>(new Range(0.78f, 0.82f), new BiomeObject(0xb05, 0xa09)),
                        new RangePair<BiomeObject>(new Range(0.82f, 0.9f), new BiomeObject(0xb05, 0xa02)),
                        new RangePair<BiomeObject>(new Range(0.9f, 0.92f), new BiomeObject(0xb05, 0xa03)),
                        new RangePair<BiomeObject>(new Range(0.95f, float.MaxValue), new BiomeObject(0, 0xa04))
                    })
                };
            case BiomeType.Lake:
                return new Biome()
                {
                    type = type,
                    displayColor = new Color(40 / 255f, 58 / 255f, 105 / 255f, 1),
                    perlinScale = 120,
                    tiles = new RangeMap<ushort>(new RangePair<ushort>[]
                    {
                        new RangePair<ushort>(new Range(float.MinValue, 0.22f), 0xb24), // Deep lake
                        new RangePair<ushort>(new Range(0.22f, 0.75f), 0xb25), // Lake
                        new RangePair<ushort>(new Range(0.75f, 0.8f), 0xb26), // Shallow lake
                        new RangePair<ushort>(new Range(0.8f, 0.84f), 0xb27), // Lake Rocks
                        new RangePair<ushort>(new Range(0.84f, float.MaxValue), 0xb28), // Lake Grass
                    }),
                    objects = new RangeMap<BiomeObject>(new RangePair<BiomeObject>[]
                    {
                        new RangePair<BiomeObject>(new Range(float.MinValue, 0.01f), new BiomeObject(0xb24, 0xa43)), // Log
                        new RangePair<BiomeObject>(new Range(0.01f, 0.025f), new BiomeObject(0xb24, 0xa42)), // Lillypad
                        new RangePair<BiomeObject>(new Range(0.03f, 0.04f), new BiomeObject(0xb25, 0xa43)), // Log
                        new RangePair<BiomeObject>(new Range(0.04f, 0.055f), new BiomeObject(0xb25, 0xa42)), // Lillypad
                        new RangePair<BiomeObject>(new Range(0.96f, float.MaxValue), new BiomeObject(0xb28, 0xa44)), // Lake tree
                    })
                };
            case BiomeType.Grasslands:
                return new Biome()
                {
                    type = type,
                    displayColor = new Color(54f / 255f, 109f / 255f, 63f / 255f, 1),
                    tiles = new RangeMap<ushort>(new RangePair<ushort>[]
                    {
                        new RangePair<ushort>(new Range(float.MinValue, float.MaxValue), 0xb02)
                    }),
                    objects = new RangeMap<BiomeObject>(new RangePair<BiomeObject>[]
                    {
                        new RangePair<BiomeObject>(new Range(0.9f, 0.92f), new BiomeObject(0xb02, 0xa0b)),
                        new RangePair<BiomeObject>(new Range(0.92f, 0.96f), new BiomeObject(0xb02, 0xa0a)),
                        new RangePair<BiomeObject>(new Range(0.96f, float.MaxValue), new BiomeObject(0xb02, 0xa07))
                    })
                };
            case BiomeType.BeachSpawn:
                var biome = CreateBiome(BiomeType.Beach);
                biome.type = type;
                biome.displayColor = Color.green;
                return biome;
            case BiomeType.Beach:
                return new Biome()
                {
                    type = type,
                    displayColor = new Color(118f / 255f, 96f / 255f, 71f / 255f, 1),
                    tiles = new RangeMap<ushort>(new RangePair<ushort>[]
                    {
                        new RangePair<ushort>(new Range(float.MinValue, float.MaxValue), 0xb04)
                    }),
                    objects = new RangeMap<BiomeObject>(new RangePair<BiomeObject>[]
                    {
                        new RangePair<BiomeObject>(new Range(0.979f, 0.988f), new BiomeObject(0, 0xa08)), // Seaweed
                        new RangePair<BiomeObject>(new Range(0.988f, 0.997f), new BiomeObject(0, 0xa01)), // Small Rocks
                        new RangePair<BiomeObject>(new Range(0.997f, float.MaxValue), new BiomeObject(0, 0xa49)), // Mossy Rock
                    })
                };
            default:
                return new Biome()
                {
                    type = BiomeType.Ocean,
                    displayColor = new Color(44f / 255f, 90f / 255f, 148f / 255f, 1),
                    tiles = new RangeMap<ushort>(new RangePair<ushort>[] 
                    {
                        new RangePair<ushort>(new Range(float.MinValue, float.MaxValue), 0xb03)
                    }),
                    objects = new RangeMap<BiomeObject>(new RangePair<BiomeObject>[]
                    {
                        new RangePair<BiomeObject>(new Range(0.985f, float.MaxValue), new BiomeObject(0, 0xa08))
                    })
                };
        }
    }

    public static Biome Get(BiomeType type)
    {
        return biomes[(int)type];
    }

    public BiomeType type;

    public Color displayColor;

    public float perlinScale;

    public RangeMap<ushort> tiles = new RangeMap<ushort>();

    public RangeMap<BiomeObject> objects = new RangeMap<BiomeObject>();
}

public struct BiomeObject
{
    public ushort tileType;

    public ushort[] objectTypes;

    public BiomeObject(ushort tileType, params ushort[] objectType)
    {
        this.tileType = tileType;
        objectTypes = objectType;
    }

    public ushort Get(ushort tileType)
    {
        if (this.tileType == 0) return GetObjectType();
        return (this.tileType == tileType) ? GetObjectType() : (ushort)0;
    }

    private ushort GetObjectType()
    {
        if (objectTypes == null || objectTypes.Length == 0) return 0;
        return objectTypes.Random();
    }
}