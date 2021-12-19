using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TitanCore.Core;
using TitanCore.Files;
using UnityEngine;
using Utils.NET.Algorithms.Voronoi;
using Utils.NET.Algorithms.Voronoi.Nodes;
using Utils.NET.Geometry;
using Utils.NET.IO;
using Utils.NET.Partitioning;
using Utils.NET.Utils;
using static TitanCore.Files.MapElementFile;

public class WorldCreator : MonoBehaviour
{
    public GameObject worldChunkPrefab;

    public Dictionary<Center, WorldChunk> chunks = new Dictionary<Center, WorldChunk>();

    private ArrayPartitionMap<WorldChunk> chunksPartition = new ArrayPartitionMap<WorldChunk>(1000, 1000, 10);

    public BiomeType drawBiome = BiomeType.Beach;

    private void Start()
    {
        int bc = 0;
        while (bc++ < 10)
        {
            try
            {
                Create();
                bc = 10;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }
    }

    public void Create()
    {
        Vec2[] positions = new Vec2[5000];
        for (int i = 0; i < positions.Length; i++)
            positions[i] = new Vec2(UnityEngine.Random.Range(0, 1.0f), UnityEngine.Random.Range(0, 1.0f));

        var graph = new VoronoiSet(positions, 1);
        foreach (var center in graph.centers)
            CreateWorldChunk(center);
    }

    private void CreateWorldChunk(Center center)
    {
        if (!ValidateCenter(center)) return;

        var chunk = Instantiate(worldChunkPrefab).GetComponent<WorldChunk>();
        chunk.transform.position = new Vector3(0, 0, 0);
        chunks.Add(center, chunk);
        chunk.Init(this, center);

        chunksPartition.Add(chunk);
    }

    private bool ValidateCenter(Center center)
    {
        foreach (var corner in center.corners)
            if (corner.position.x < 0 || corner.position.x > 1 || corner.position.y < 0 || corner.position.y > 1)
                return false;

        return true;
    }

    public IEnumerable<WorldChunk> GetNeighbors(Center center)
    {
        foreach (var neighbor in center.neighbors)
        {
            if (!chunks.TryGetValue(neighbor, out var value)) continue;
            yield return value;
        }
    }

    public byte[] Export(int size)
    {
        var stream = new MemoryStream();
        var file = ExportMap(size);
        file.Write(stream);
        return stream.ToArray();
    }

    private WorldChunk ClosestChunk(float x, float y)
    {
        return chunksPartition.GetNearObjects(new Utils.NET.Geometry.Rect(x * 1000, y * 1000, 0, 0)).Closest(_ => (_.Position * 1000 - new Vec2(x * 1000, y * 1000)).SqrLength);
    }

    private Biome BiomeAt(float x, float y)
    {
        try
        {
            return Biome.Get(ClosestChunk(x, y).biomeType);
        }
        catch
        {
            return Biome.Get(BiomeType.Ocean);
        }
    }

    private ushort TileAt(float x, float y)
    {
        var biome = BiomeAt(x, y);
        return biome.tiles[Mathf.PerlinNoise(x * biome.perlinScale, y * biome.perlinScale)];
    }

    private ushort ObjectAt(ushort tileType, float x, float y)
    {
        var biome = BiomeAt(x, y);
        return biome.objects?.Get(UnityEngine.Random.Range(0f, 1f)).Get(tileType) ?? 0;
    }

    private MapElementFile ExportMap(int size)
    {
        var mapFile = new MapElementFile();
        mapFile.width = size;
        mapFile.height = size;
        mapFile.tiles = new MapTileElement[size, size];
        mapFile.entities = new MapEntityElement[0];

        var regions = new List<MapRegionElement>();

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                var tileType = TileAt(x / (float)size, y / (float)size);
                mapFile.tiles[x, y] = new MapTileElement
                {
                    tileType = tileType,
                    objectType = ObjectAt(tileType, x / (float)size, y / (float)size)
                };
            }
        }

        var village = chunks.Values.First(_ => _.biomeType == BiomeType.BeachSpawn);
        var center = village.Position * size;
        regions.Add(new MapRegionElement()
        {
            x = (uint)center.x,
            y = (uint)center.y,
            regionType = Region.Spawn
        });

        mapFile.regions = regions.ToArray();

        return mapFile;
    }
}
