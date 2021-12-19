using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils.NET.Algorithms.Voronoi.Nodes;
using Utils.NET.Geometry;
using Utils.NET.IO;
using Utils.NET.Partitioning;
using Utils.NET.Pathfinding;

public class WorldChunk : MonoBehaviour, IPathNode<WorldChunk>, IPartitionable
{
    private WorldCreator creator;

    private Center center;

    public Vec2 Position => center.position;

    public IEnumerable<WorldChunk> Adjacent => creator.GetNeighbors(center);

    public Utils.NET.Geometry.Rect BoundingRect => Utils.NET.Geometry.Rect.FromBounds(center.corners.Select(_ => _.position * 1000));

    public IntRect LastBoundingRect { get; set; }


    public BiomeType biomeType = BiomeType.Ocean;

    public void Init(WorldCreator creator, Center center)
    {
        this.creator = creator;
        this.center = center;

        var meshFilter = GetComponent<MeshFilter>();
        if (meshFilter.mesh != null)
            Destroy(meshFilter.mesh);

        meshFilter.mesh = CreateMesh(center);

        gameObject.AddComponent<MeshCollider>();
    }

    public void SetBiome(BiomeType type)
    {
        if (biomeType == BiomeType.Ocean && type != BiomeType.Beach) return;
        if (biomeType == BiomeType.Beach && type != BiomeType.Ocean && type != BiomeType.BeachSpawn && Adjacent.Any(_ => _.biomeType == BiomeType.Ocean)) return;

        biomeType = type;

        var biome = Biome.Get(type);
        SetColor(biome.displayColor);
    }

    private static WorldChunk lastDrawn;

    private void OnMouseEnter()
    {
        if (Input.GetMouseButton(0))
        {
            Draw();
        }
    }
    
    private void OnMouseDown()
    {
        lastDrawn = null;
        if (Input.GetMouseButton(1))
        {
            Fill();
        }
        Draw();
    }

    private void Draw()
    {
        foreach (var node in AStar.Pathfind(lastDrawn ?? this, this))
        {
            node.SetBiome(creator.drawBiome);
        }
        lastDrawn = this;
    }

    private void Fill()
    {
        var targetBiome = biomeType;
        var fillBiome = creator.drawBiome;

        if (targetBiome == fillBiome) return;

        var filled = new HashSet<WorldChunk>();
        var toFill = new Queue<WorldChunk>();
        toFill.Enqueue(this);

        while (toFill.Count > 0)
        {
            var chunk = toFill.Dequeue();
            chunk.SetBiome(fillBiome);

            if (chunk.biomeType != fillBiome) continue;

            foreach (var neighbor in chunk.Adjacent)
            {
                if (neighbor.biomeType == targetBiome && filled.Add(neighbor))
                {
                    toFill.Enqueue(neighbor);
                }
            }
        }
    }

    private Mesh CreateMesh(Center center)
    {
        var mesh = new Mesh();

        var vertices = new Vector3[center.corners.Count];
        var corners = center.corners.OrderBy(_ => -center.position.AngleTo(_.position));
        int cornerIndex = 0;
        foreach (var corner in corners)
            vertices[cornerIndex++] = new Vector3(corner.position.x, corner.position.y, 0);

        int[] triangles = new int[(vertices.Length - 2) * 3];
        int triCount = 0;
        for (int i = 1; i < vertices.Length - 1; i++)
        {
            triangles[triCount++] = 0;
            triangles[triCount++] = i;
            triangles[triCount++] = i + 1;
        }

        var color = Biome.Get(biomeType).displayColor;
        var colors = new Color[vertices.Length];
        for (int i = 0; i < colors.Length; i++)
            colors[i] = color;

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;

        return mesh;
    }

    private void SetColor(Color color)
    {
        var meshFilter = GetComponent<MeshFilter>();
        if (meshFilter.mesh == null) return;

        var colors = new Color[meshFilter.mesh.vertices.Length];
        for (int i = 0; i < colors.Length; i++)
            colors[i] = color;

        meshFilter.mesh.colors = colors;
    }
}
