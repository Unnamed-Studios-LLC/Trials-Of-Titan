using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Utils.NET.Utils;
using Utils.NET.Geometry;

public enum MeshFace
{
    Down = 0,
    Right = 1,
    Top = 2,
    Left = 3,
    Up = 4
}

public class MeshGroup
{
    public Mesh mesh;

    public Mesh[] faces;

    public Color meanColor;

    public Color modeColor;

    public MeshGroup(Mesh mesh, Mesh[] faces, Color meanColor, Color modeColor)
    {
        this.mesh = mesh;
        this.faces = faces;
        this.meanColor = meanColor;
    }
}

public static class MeshManager
{
    /// <summary>
    /// Quad meshes ready to be assigned to sprites
    /// </summary>
    private static Dictionary<string, MeshGroup> meshes = new Dictionary<string, MeshGroup>();

    private static Vector3 defaultScalar = new Vector3(1, 1, 1);

    private static Vector3 topScalar = new Vector3(1, 1, 1);

    /// <summary>
    /// Initializes the game's meshes
    /// </summary>
    /// <param name="meshes"></param>
    public static void Init(Texture2D texture, params Mesh[] meshes)
    {
        MeshManager.meshes.Clear();
        foreach (var mesh in meshes)
        {
            AddMesh(mesh.name, mesh, texture);
        }
    }

    /// <summary>
    /// Adds a given mesh to the mesh library
    /// </summary>
    /// <param name="name"></param>
    /// <param name="mesh"></param>
    private static void AddMesh(string name, Mesh mesh, Texture2D texture)
    {
        GetAverageColors(mesh, texture, out var mean, out var mode);
        meshes.Add(name, new MeshGroup(mesh, SplitMesh(mesh), mean, mode));
    }

    /// <summary>
    /// Returns a mesh for a given name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static MeshGroup GetMesh(string name)
    {
        meshes.TryGetValue(name, out var mesh);
        return mesh;
    }

    private static int[] triangles = new int[] { 0, 1, 2, 0, 2, 3 };

    private static Mesh[] SplitMesh(Mesh mesh)
    {
        try
        {
            return new Mesh[]
            {
            GetFaceMesh(0, mesh, defaultScalar),
            GetFaceMesh(1, mesh, defaultScalar),
            GetFaceMesh(2, mesh, topScalar),
            GetFaceMesh(3, mesh, defaultScalar),
            GetFaceMesh(4, mesh, defaultScalar)
            };
        }
        catch
        {
            return new Mesh[0];
        }
    }

    private static Mesh GetFaceMesh(int faceIndex, Mesh mesh, Vector3 scalar)
    {
        int face = faceIndex * 4;
        var newMesh = new Mesh();
        var verts = new Vector3[4];
        for (int i = 0; i < verts.Length; i++)
        {
            verts[i] = ScaleVertex(mesh.vertices[face + i], scalar);
        }
        newMesh.vertices = verts;
        newMesh.triangles = triangles;
        newMesh.uv = new Vector2[]
        {
            mesh.uv[face],
            mesh.uv[face + 1],
            mesh.uv[face + 2],
            mesh.uv[face + 3]
        };
        return newMesh;
    }

    private static Vector3 ScaleVertex(Vector3 vertex, Vector3 scalar)
    {
        return new Vector3(vertex.x * scalar.x, vertex.y * scalar.y, vertex.z * scalar.z);
    }

    private static void GetAverageColors(Mesh mesh, Texture2D texture, out Color mean, out Color mode)
    {
        var pixels = GetPixels(mesh, texture);
        var colorList = new List<Color>();
        var modeCount = new Dictionary<Color, int>();

        float r, g, b;
        r = g = b = 0;
        foreach (var color in pixels)
        {
            if (color.a <= 0) continue;
            colorList.Add(color);
            r += color.r;
            g += color.g;
            b += color.b;

            if (!modeCount.TryGetValue(color, out var count))
                count = 0;
            modeCount[color] = ++count;
        }
        //colors = colorList.ToArray();

        mean = new Color(r / colorList.Count, g / colorList.Count, b / colorList.Count, 1);
        mode = modeCount.Closest(_ => -_.Value).Key;
    }

    private static Color[] GetPixels(Mesh mesh, Texture2D texture)
    {
        var colors = new List<Color>();
        var triangles = mesh.triangles;
        var uvs = mesh.uv;
        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            var uv0 = uvs[mesh.triangles[i]].ToVec2();
            var uv1 = uvs[mesh.triangles[i + 1]].ToVec2();
            var uv2 = uvs[mesh.triangles[i + 2]].ToVec2();
            var triangle = new Triangle(uv0, uv1, uv2);

            foreach (var point in triangle.GetPoints())
                colors.Add(texture.GetPixel((int)(point.x * texture.width), (int)(point.y * texture.height)));
        }
        return colors.ToArray();
    }
}
