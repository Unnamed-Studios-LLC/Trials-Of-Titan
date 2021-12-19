using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TitanCore.Data;
using TitanCore.Net.Packets;
using TitanCore.Net.Packets.Models;
using UnityEngine;
using UnityEngine.UI;
using Utils.NET.Collections;
using Utils.NET.Geometry;
using Utils.NET.Utils;

[UnityEngine.ExecuteInEditMode]
public class MapGen : UnityEngine.MonoBehaviour
{
    /*
    private const int Map_Size = 1000;

    private struct MapTriangle
    {
        public Center center;

        public Corner[] corners;
    }

    public Camera mainCamera;

    public MeshFilter difficultyFilter;

    public MeshFilter townRegionsFilter;

    public MeshFilter elevationFilter;

    public RawImage renderImage;

    public MeshFilter[] riverRenderers;

    public GameObject riverRendererPrefab;

    public MeshFilter[] roadRenderers;

    public GameObject roadRendererPrefab;

    public RawImage image;

    public int seed = 123456789;

    public int pointCount = 5000;

    public int relaxations = 2;

    [Range(0, 1)]
    public float bias = 0.5f;

    private WorldGen.World world;

    private Color waterColor = Color.clear;//new Color(54f / 255f, 58f / 255f, 126f / 255f);

    private Color landColor = new Color(172f / 255f, 159f / 255f, 139f / 255f);

    private List<LineRenderer> riverLineRenderers = new List<LineRenderer>();

    private List<LineRenderer> roadLineRenderers = new List<LineRenderer>();

    public void Generate()
    {
        if (image.texture != null)
            DestroyImmediate(image.texture);

        world = WorldGen.WorldGen.Generate(Map_Size, Map_Size, seed, relaxations, pointCount);
        mainCamera.orthographicSize = Map_Size / 2;
        difficultyFilter.transform.position = new Vector3(-Map_Size / 4, Map_Size / 4, 0);
        townRegionsFilter.transform.position = new Vector3(Map_Size / 4, Map_Size / 4, 0);
        elevationFilter.transform.position = new Vector3(-Map_Size / 4, -Map_Size / 4, 0);

        riverRendererPrefab.GetComponent<LineRenderer>().widthCurve = new AnimationCurve(new Keyframe(0, Map_Size / 40));
        roadRendererPrefab.GetComponent<LineRenderer>().widthCurve = new AnimationCurve(new Keyframe(0, Map_Size / 40));

        //GenerateTexture();

        GenerateMeshes();

        GenerateRivers();
        GenerateRoads();
    }

    private bool HandleTest(TnPacket p)
    {
        return false;
    }

    private void GenerateMeshes()
    {
        if (difficultyFilter.sharedMesh != null)
            DestroyImmediate(difficultyFilter.sharedMesh);

        if (townRegionsFilter.sharedMesh != null)
            DestroyImmediate(townRegionsFilter.sharedMesh);

        difficultyFilter.sharedMesh = GenerateMesh(GetDifficultyColors);
        townRegionsFilter.sharedMesh = GenerateMesh(GetTownRegionColors);
        elevationFilter.sharedMesh = GenerateMesh(GetElevationColors);

        if (renderImage.texture != null)
            Destroy(renderImage.texture);

        var worldDefinition = new WorldDefinition
        {
            ocean = new BiomeDefinition(400, new RangePair<ushort>[]
                {
                    new RangePair<ushort>(new Range(0, 1), 0xb03)
                }, null),
            beach = new BiomeDefinition(400, new RangePair<ushort>[]
                {
                    new RangePair<ushort>(new Range(0, 1), 0xb04)
                }, null)
        };

        worldDefinition.biomes.Add(new Range(0, 0.35f), new BiomeDefinition(400, new RangePair<ushort>[] // Grasslands
        {
                new RangePair<ushort>(new Range(0, 0.75f), 0xb02),
                new RangePair<ushort>(new Range(0.75f, 1), 0xb00)
        }, null));

        worldDefinition.biomes.Add(new Range(0.35f, 0.5f), new BiomeDefinition(600, new RangePair<ushort>[] // Wetlands
        {
                new RangePair<ushort>(new Range(0, 0.75f), 0xb05),
                new RangePair<ushort>(new Range(0.75f, 1), 0xb06)
        },
        new RangePair<ushort>[]
        {
                new RangePair<ushort>(new Range(0.7f, 0.75f), 0xa04),
                new RangePair<ushort>(new Range(0.64f, 0.65f), 0xa03),
                new RangePair<ushort>(new Range(0.65f, 0.7f), 0xa02)
        }));

        worldDefinition.biomes.Add(new Range(0.5f, 0.6f), new BiomeDefinition(400, new RangePair<ushort>[] // Desert
        {
                new RangePair<ushort>(new Range(0, 0.75f), 0xb07),
                new RangePair<ushort>(new Range(0.75f, 1), 0xb08)
        },
        new RangePair<ushort>[]
        {
                new RangePair<ushort>(new Range(0.71f, 0.75f), 0xa05)
        }));

        worldDefinition.biomes.Add(new Range(0.8f, 1f), new BiomeDefinition(400, new RangePair<ushort>[] // Lava
        {
                new RangePair<ushort>(new Range(0, 0.75f), 0xb07),
                new RangePair<ushort>(new Range(0.75f, 1), 0xb08)
        },
        new RangePair<ushort>[]
        {
                new RangePair<ushort>(new Range(0.71f, 0.75f), 0xa05)
        }));

        worldDefinition.biomes.Add(new Range(0.8f, 1f), new BiomeDefinition(300, new RangePair<ushort>[] // Snow
        {
                new RangePair<ushort>(new Range(0, 0.75f), 0xb07),
                new RangePair<ushort>(new Range(0.75f, 1), 0xb08)
        },
        new RangePair<ushort>[]
        {
                new RangePair<ushort>(new Range(0.71f, 0.75f), 0xa05)
        }));

        worldDefinition.biomes.Add(new Range(0.5f, 1), new BiomeDefinition(400, new RangePair<ushort>[] // Undefined
        {
                new RangePair<ushort>(new Range(0, 1), 0xb00)
        }, null));

        renderImage.texture = CreateRenderTexture(world.Rasterize(worldDefinition));
    }

    private Mesh GenerateMesh(Func<MapTriangle[], IEnumerable<Color>> colorSelector)
    {
        var triangles = GenerateTriangles();

        var mesh = new Mesh();
        mesh.vertices = GetVertices(triangles).ToArray();
        mesh.triangles = GetTriangles(triangles).ToArray();
        mesh.colors = colorSelector(triangles).ToArray();
        return mesh;
    }

    private void GenerateRivers()
    {
        int i = 0;

        foreach (var render in riverRenderers)
        {
            foreach (var river in world.rivers)
            {
                var positions = GetRiver(river).Select(_ => new Vector3(_.position.x - world.width / 2, _.position.y - world.height / 2)).ToArray();

                LineRenderer renderer = null;
                if (i < riverLineRenderers.Count)
                {
                    renderer = riverLineRenderers[i++];
                }
                else
                {
                    renderer = Instantiate(riverRendererPrefab).GetComponent<LineRenderer>();
                    riverLineRenderers.Add(renderer);
                    i++;
                }
                renderer.gameObject.SetActive(true);

                renderer.positionCount = positions.Length;
                renderer.SetPositions(positions);

                renderer.transform.SetParent(render.transform);
                renderer.transform.localPosition = new Vector3(0, 0, -1);
            }
        }

        for (int x = i; x < riverLineRenderers.Count; x++)
        {
            DestroyImmediate(riverLineRenderers[x].gameObject);
        }
        if (i < riverLineRenderers.Count)
            riverLineRenderers.RemoveRange(i, riverLineRenderers.Count - i);
    }

    private void GenerateRoads()
    {
        int i = 0;

        foreach (var render in roadRenderers)
        {
            foreach (var road in world.roads)
            {
                var positions = road.Select(_ => new Vector3(_.x - world.width / 2, _.y - world.height / 2)).ToArray();

                LineRenderer renderer = null;
                if (i < roadLineRenderers.Count)
                {
                    renderer = roadLineRenderers[i++];
                }
                else
                {
                    renderer = Instantiate(roadRendererPrefab).GetComponent<LineRenderer>();
                    roadLineRenderers.Add(renderer);
                    i++;
                }
                renderer.gameObject.SetActive(true);

                renderer.positionCount = positions.Length;
                renderer.SetPositions(positions);

                renderer.transform.SetParent(render.transform);
                renderer.transform.localPosition = new Vector3(0, 0, -1);
            }
        }

        for (int x = i; x < roadLineRenderers.Count; x++)
        {
            DestroyImmediate(roadLineRenderers[x].gameObject);
        }
        if (i < roadLineRenderers.Count)
            roadLineRenderers.RemoveRange(i, roadLineRenderers.Count - i);
    }

    private HashSet<Corner> GetRiver(Corner river)
    {
        HashSet<Corner> corners = new HashSet<Corner>();
        while (river != null)
        {
            if (!corners.Add(river))
                break;
            river = river.river;
        }
        return corners;
    }

    public void GenerateTexture()
    {
        if (!image.enabled) return;

        var tex = new Texture2D(Map_Size, Map_Size);

        for (int y = 0; y < tex.height; y++)
            for (int x = 0; x < tex.width; x++)
            {
                var center = world.GetCenterNear(x, y);
                if (center != null && !center.water)
                {
                    var displayValue = center.difficulty;
                    if (bias > 0.5f)
                        displayValue = Mathf.Lerp(displayValue, 1, (bias - 0.5f) / 0.5f);
                    else
                        displayValue = Mathf.Lerp(0, displayValue, bias / 0.5f);

                    if (displayValue < 0.5f)
                        tex.SetPixel(x, y, Color.Lerp(Color.green, Color.yellow, displayValue / 0.5f));
                    else
                        tex.SetPixel(x, y, Color.Lerp(Color.yellow, Color.red, (displayValue - 0.5f) / 0.5f));
                }
                else
                    tex.SetPixel(x, y, waterColor);
            }
        tex.Apply();

        image.texture = tex;
        
    }

    private MapTriangle[] GenerateTriangles()
    {
        var triangles = new List<MapTriangle>();
        foreach (var center in world.landCenters)
        {
            var border = center.corners.OrderBy(_ => -(_.position - center.position).Angle).ToArray();
            int triCount = border.Length - 2;

            for (int i = 0; i < triCount; i++)
            {
                triangles.Add(new MapTriangle
                {
                    center = center,
                    corners = new Corner[]
                    {
                        border[0],
                        border[i + 1],
                        border[i + 2]
                    }
                });
            }
        }
        return triangles.ToArray();
    }

    private IEnumerable<Vector3> GetVertices(MapTriangle[] triangles)
    {
        foreach (var triangle in triangles)
            foreach (var vertex in triangle.corners)
                yield return new Vector3(vertex.position.x - world.width / 2, vertex.position.y - world.height / 2, 0);
    }

    private IEnumerable<Color> GetDifficultyColors(MapTriangle[] triangles)
    {
        foreach (var triangle in triangles)
            foreach (var vertex in triangle.corners)
            {
                if (triangle.center.water)
                {
                    yield return waterColor;
                    continue;
                }

                if (triangle.center.town)
                {
                    yield return Color.blue;
                    continue;
                }

                var displayValue = triangle.center.difficulty;
                if (bias > 0.5f)
                    displayValue = Mathf.Lerp(displayValue, 1, (bias - 0.5f) / 0.5f);
                else
                    displayValue = Mathf.Lerp(0, displayValue, bias / 0.5f);

                if (displayValue < 0.5f)
                    yield return Color.Lerp(Color.green, Color.yellow, displayValue / 0.5f);
                else
                    yield return Color.Lerp(Color.yellow, Color.red, (displayValue - 0.5f) / 0.5f);
            }
    }

    private IEnumerable<Color> GetTownRegionColors(MapTriangle[] triangles)
    {
        var townColors = world.landCenters.Where(_ => _.town).OrderBy(_ => _.difficulty).ToDictionary(_ => _, _ => new Color(_.difficulty, _.difficulty, _.difficulty, 1));
        foreach (var triangle in triangles)
            foreach (var vertex in triangle.corners)
            {
                yield return townColors[triangle.center.townRegion];
            }
    }

    private IEnumerable<Color> GetElevationColors(MapTriangle[] triangles)
    {
        foreach (var triangle in triangles)
            foreach (var vertex in triangle.corners)
            {
                var elevation = triangle.center.elevation;
                yield return new Color(elevation, elevation, elevation, 1);
            }
    }

    private Texture CreateRenderTexture(MapTile[,] tiles)
    {
        var colors = new Color[Map_Size * Map_Size];
        for (int y = 0; y < Map_Size; y++)
        {
            for (int x = 0; x < Map_Size; x++)
            {
                var tile = tiles[x, y];
                if (tile.tileType == 0)
                    colors[y * Map_Size + x] = Color.black;
                else
                {
                    var info = GameData.objects[tile.tileType];
                    var sprite = TextureManager.GetSprite(info.textures[0].displaySprite);
                    var metaData = TextureManager.GetMetaData(sprite);
                    colors[y * Map_Size + x] = metaData.averageColor;
                }
            }
        }

        var texture = new Texture2D(Map_Size, Map_Size);
        texture.filterMode = FilterMode.Point;
        texture.SetPixels(colors);
        texture.Apply();
        return texture;
    }

    private IEnumerable<int> GetTriangles(MapTriangle[] triangles)
    {
        int i = 0;
        foreach (var triangle in triangles)
        {
            yield return i++;
            yield return i++;
            yield return i++;
        }
    }

    private void OnValidate()
    {
        if (world != null)
        {
            //GenerateTexture();
            GenerateMeshes();
        }
    }
    */
}