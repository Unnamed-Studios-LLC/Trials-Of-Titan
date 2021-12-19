using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TitanCore.Files;
using TitanCore.Gen;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Utils.NET.Collections;
using Utils.NET.Geometry;
using Utils.NET.Utils;
using static TitanCore.Files.MapElementFile;

public class GateGenerator : MonoBehaviour
{
    public enum Function
    {
        Cellular,
        Perlin
    }

    public RawImage image;

    [Range(0, 1)]
    public float percentage;

    public int width;

    public int height;

    public int seed;

    public int smoothIterations;

    public int emptyMassBelow = 0;

    public int groundMassBelow = 0;

    public Function function = Function.Cellular;

    public int smoothingRange = 1;

    public int maxLandmass = 1;

    public int extrude = 0;

    public int wallThickness = 1;

    public float perlinScale = 1;

    public float perlinOffset;

    public float sizeMin;

    public float sizeMax;

    public Vector2 scale = new Vector2(1, 1);

    private void Start()
    {
        Generate();
    }

    public void Generate(bool randomSeed = false)
    {
        GenMap genMap;
        int count = 0;
        int bc = 0;
        do
        {
            if (randomSeed)
                seed = UnityEngine.Random.Range(0, int.MaxValue);

            switch (function)
            {
                case Function.Perlin:
                    genMap = new PerlinMap(width, height, perlinOffset, perlinScale, percentage, smoothIterations, emptyMassBelow, groundMassBelow, smoothingRange, maxLandmass, extrude, wallThickness);
                    break;
                default:
                    genMap = new CellularMap(width, height, seed, percentage, smoothIterations, emptyMassBelow, groundMassBelow, smoothingRange, maxLandmass, extrude, wallThickness);
                    break;
            }
            genMap.Generate();
            var groundMasses = genMap.GetLandmasses(maxLandmass);

            count = 0;
            foreach (var mass in groundMasses)
                foreach (var point in mass)
                    count++;
        }
        while ((count < sizeMin || count > sizeMax) && ++bc < 20 && randomSeed);
        Debug.Log(count);

        if (bc == 20)
        {
            Debug.LogError("Failed to generate within the size limit");
        }

        var biggestArea = genMap.GetBiggestAreas(1, maxLandmass).First();

        var distanceField = genMap.GetDistanceField(biggestArea.First(), out var m);

        /*
        var rnd = eligableSpots.ToArray().Random();
        genMap.Set(rnd, MapElementType.Wall);

        foreach (var p in genMap.Spread(rnd))
        {
            if (p.DistanceTo(rnd) > 10) break;
            var type = genMap.Get(p);
            if (type != MapElementType.Ground) continue;
            genMap.Set(p, MapElementType.Wall);
        }
        */

        var clamp = genMap.Scale(scale.ToVec2()).Clamp();
        var max = Mathf.Max(clamp.width, clamp.height);
        var map = new Map(max, max, clamp);

        var colors = new Color32[map.width * map.height];
        for (int y = 0; y < map.height; y++)
            for (int x = 0; x < map.width; x++)
                colors[y * map.width + x] = ColorFor(map.Get(new Int2(x, y)));

        var texture = new Texture2D(map.width, map.height);
        texture.filterMode = FilterMode.Point;
        texture.SetPixels32(colors);
        texture.Apply();

        /*
        var colors = new Color[genMap.width * genMap.height];
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                int distance = distanceField[x, y];
                float val = (distance + 1) / (float)(maxDistance + 1);
                colors[y * width + x] = new Color(val, val, val, 255);
            }

        var texture = new Texture2D(genMap.width, genMap.height);
        texture.filterMode = FilterMode.Point;
        texture.SetPixels(colors);
        texture.Apply();
        */

        if (image.texture != null)
        {
            DestroyImmediate(image.texture);
        }
        image.texture = texture;
    }

    private Color32 ColorFor(MapElementType type)
    {
        switch (type)
        {
            case MapElementType.Ground:
                return Color.white;
            case MapElementType.Wall:
                return Color.red;
            case MapElementType.SetPiece:
                return Color.green;
            default:
                return Color.black;
        }
    }

    public void ExportShape()
    {
#if UNITY_EDITOR
        var texture = (Texture2D)image.texture;

        var mapFile = new MapElementFile();
        mapFile.width = texture.width;
        mapFile.height = texture.height;

        var colors = texture.GetPixels();

        mapFile.tiles = new MapTileElement[mapFile.width, mapFile.height];
        for (int y = 0; y < mapFile.height; y++)
            for (int x = 0; x < mapFile.width; x++)
            {
                var color = colors[y * mapFile.width + x];
                if (color.r > 0) {
                    mapFile.tiles[x, y] = new MapTileElement()
                    {
                        tileType = 0xb00
                    };
                }
            }

        mapFile.entities = new MapEntityElement[0];
        mapFile.regions = new MapRegionElement[0];

        string filePath = EditorUtility.SaveFilePanel("Save Map Shape", "", "", "mef");
        if (string.IsNullOrWhiteSpace(filePath)) return;
        if (File.Exists(filePath))
            File.Delete(filePath);
        var stream = File.OpenWrite(filePath);
        mapFile.Write(stream);
#endif
    }
}