using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Utils.NET.Collections;

[ExecuteInEditMode]
public class MapGenRender : MonoBehaviour
{
    public int seed = 123456789;

    public int mapSize = 100;

    public int pointCount = 1000;

    public void Generate()
    {
        var rawImage = GetComponent<RawImage>();

        //var world = WorldGen.WorldGen.Generate(mapSize, mapSize, seed, 1, pointCount);
        Texture newTexture = null;// CreateTexture(world);

        if (rawImage.texture != null)
            Destroy(rawImage.texture);
        rawImage.texture = newTexture;
    }

    /*
    private Texture CreateTexture(WorldGen.World world)
    {
        var texture = new Texture2D(world.width, world.height);
        texture.filterMode = FilterMode.Point;
        
        try
        {
            var worldDefinition = new WorldDefinition
            {
                ocean = new BiomeDefinition(0xb03),
                beach = new BiomeDefinition(0xb04)
            };
            worldDefinition.biomes.Add(new Range(0, 0.3f), new BiomeDefinition(0xb02));
            worldDefinition.biomes.Add(new Range(0.3f, 0.5f), new BiomeDefinition(0xb05));
            worldDefinition.biomes.Add(new Range(0.5f, 1), new BiomeDefinition(0xb00));

            var tiles = world.Rasterize(worldDefinition);


        }
        catch
        {
            Destroy(texture);
            return null;
        }

        return texture;
    }
    */
}
