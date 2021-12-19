using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Data;
using TitanCore.Data.Map;
using UnityEngine;
using Utils.NET.Geometry;
using Utils.NET.Partitioning;

public abstract class GroundChunk<T> : MonoBehaviour, IChunk 
    where T : IChunk
{
    private const float VERTEX_OFFSET = 0.001f;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh mesh;
    private Vector2[] uvs;
    private Vector2[] blendUvs;
    private Vector2[] blendMaskUvs;

    private bool uvsChanged = false;
    private int displayedTilesCount = 0;

    private int x;
    private int y;

    protected TilemapManager manager;
    protected int chunkSize;

    public virtual void Initialize(TilemapManager manager, int chunkSize, int sortOrder)
    {
        this.manager = manager;
        this.chunkSize = chunkSize;

        meshFilter = GetComponent<MeshFilter>();

        mesh = CreateMesh(chunkSize, chunkSize);
        meshFilter.mesh = mesh;

        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.sharedMaterial.mainTexture = TextureManager.GetSprite(GameData.objects.Values.First(_ => _ is TileInfo).textures[0].displaySprite).texture;
        meshRenderer.sortingLayerName = "Tilemap";
        meshRenderer.sortingLayerID = SortingLayer.NameToID("Tilemap");
        meshRenderer.sortingOrder = sortOrder;
    }

    private Mesh CreateMesh(int width, int height)
    {
        Mesh newMesh = new Mesh();

        newMesh.vertices = CreateVertices(width, height);
        newMesh.triangles = CreateIndices(width, height);
        uvs = CreateUvs(width, height);
        blendUvs = CreateUvs(width, height);
        blendMaskUvs = CreateUvs(width, height);
        newMesh.uv = uvs;
        newMesh.uv2 = blendUvs;
        newMesh.uv3 = blendMaskUvs;

        return newMesh;
    }

    private Vector3[] CreateVertices(int meshWidth, int meshHeight)
    {
        Vector3[] vertices = new Vector3[meshWidth * meshHeight * 4];
        for (int y = 0; y < meshHeight; y++)
        {
            for (int x = 0; x < meshWidth; x++)
            {
                int i = (y * meshHeight + x) * 4;
                vertices[i] = new Vector3(x - VERTEX_OFFSET, y - VERTEX_OFFSET);
                vertices[i + 1] = new Vector3(x + 1 + VERTEX_OFFSET, y - VERTEX_OFFSET);
                vertices[i + 2] = new Vector3(x + 1 + VERTEX_OFFSET, y + 1 + VERTEX_OFFSET);
                vertices[i + 3] = new Vector3(x - VERTEX_OFFSET, y + 1 + VERTEX_OFFSET);
            }
        }
        return vertices;
    }

    private int[] CreateIndices(int meshWidth, int meshHeight)
    {
        int[] indices = new int[meshWidth * meshHeight * 6];
        int boxIndex = 0;
        for (int i = 0; i < indices.Length; i += 6)
        {
            int vIndex = boxIndex * 4;
            indices[i] = vIndex;
            indices[i + 1] = vIndex + 3;
            indices[i + 2] = vIndex + 2;
            indices[i + 3] = vIndex + 2;
            indices[i + 4] = vIndex + 1;
            indices[i + 5] = vIndex;
            boxIndex++;
        }
        return indices;
    }

    private Vector2[] CreateUvs(int meshWidth, int meshHeight)
    {
        Vector2[] meshUvs = new Vector2[meshWidth * meshHeight * 4];
        for (int i = 0; i < meshUvs.Length; i += 4)
        {
            meshUvs[i] = new Vector2(1, 1);
            meshUvs[i + 1] = new Vector2(1, 1);
            meshUvs[i + 2] = new Vector2(1, 1);
            meshUvs[i + 3] = new Vector2(1, 1);
        }
        return meshUvs;
    }

    protected void ApplySprite(int localX, int localY, Sprite sprite, TileRotation rotation = TileRotation.None)
    {
        var rect = sprite.textureRect;
        var textureSize = new Vector2(sprite.texture.width, sprite.texture.height);

        int index = (localY * chunkSize + localX) * 4;

        if (uvs[index] == Vector2.one)
        {
            displayedTilesCount++;
            CheckEnabled();
        }

        int i = (int)rotation;

        uvs[index + i] = new Vector2(rect.x, rect.y) / textureSize;
        i = (i + 1) % 4;

        uvs[index + i] = new Vector2(rect.x + rect.width, rect.y) / textureSize;
        i = (i + 1) % 4;

        uvs[index + i] = new Vector2(rect.x + rect.width, rect.y + rect.height) / textureSize;
        i = (i + 1) % 4;

        uvs[index + i] = new Vector2(rect.x, rect.y + rect.height) / textureSize;

        uvsChanged = true;
    }

    protected void ApplyBlend(int localX, int localY, Sprite blendSprite, Sprite blendMask, TileRotation rotation, TileRotation maskRotation)
    {
        var blendRect = blendSprite.textureRect;
        var textureSize = new Vector2(blendSprite.texture.width, blendSprite.texture.height);

        int index = (localY * chunkSize + localX) * 4;

        int i = (int)rotation;

        blendUvs[index + i] = new Vector2(blendRect.x, blendRect.y) / textureSize;
        i = (i + 1) % 4;

        blendUvs[index + i] = new Vector2(blendRect.x + blendRect.width, blendRect.y) / textureSize;
        i = (i + 1) % 4;

        blendUvs[index + i] = new Vector2(blendRect.x + blendRect.width, blendRect.y + blendRect.height) / textureSize;
        i = (i + 1) % 4;

        blendUvs[index + i] = new Vector2(blendRect.x, blendRect.y + blendRect.height) / textureSize;

        blendRect = blendMask.textureRect;
        textureSize = new Vector2(blendMask.texture.width, blendMask.texture.height);

        i = (int)maskRotation;

        blendMaskUvs[index + i] = new Vector2(blendRect.x, blendRect.y) / textureSize;
        i = (i + 1) % 4;

        blendMaskUvs[index + i] = new Vector2(blendRect.x + blendRect.width, blendRect.y) / textureSize;
        i = (i + 1) % 4;

        blendMaskUvs[index + i] = new Vector2(blendRect.x + blendRect.width, blendRect.y + blendRect.height) / textureSize;
        i = (i + 1) % 4;

        blendMaskUvs[index + i] = new Vector2(blendRect.x, blendRect.y + blendRect.height) / textureSize;

        uvsChanged = true;
    }

    protected void RemoveSprite(int localX, int localY)
    {
        int index = (localY * chunkSize + localX) * 4;

        if (uvs[index] != Vector2.one)
        {
            displayedTilesCount--;
        }

        uvs[index++] = new Vector2(1, 1);
        uvs[index++] = new Vector2(1, 1);
        uvs[index++] = new Vector2(1, 1);
        uvs[index++] = new Vector2(1, 1);

        index -= 4;

        blendMaskUvs[index++] = new Vector2(1, 1);
        blendMaskUvs[index++] = new Vector2(1, 1);
        blendMaskUvs[index++] = new Vector2(1, 1);
        blendMaskUvs[index++] = new Vector2(1, 1);

        uvsChanged = true;
    }

    protected void RemoveBlend(int localX, int localY)
    {
        int index = (localY * chunkSize + localX) * 4;

        blendMaskUvs[index++] = new Vector2(1, 1);
        blendMaskUvs[index++] = new Vector2(1, 1);
        blendMaskUvs[index++] = new Vector2(1, 1);
        blendMaskUvs[index++] = new Vector2(1, 1);

        uvsChanged = true;
    }

    private bool IsOutOfBounds()
    {
        return (x < 0 || x >= manager.width || y < 0 || y >= manager.height);
    }

    private bool CheckEnabled()
    {
        gameObject.SetActive(!IsOutOfBounds() && (displayedTilesCount != 0));
        return gameObject.activeSelf;
    }

    public virtual void LateUpdate()
    {
        if (!gameObject.activeSelf) return;
        FlushUvs();
    }

    public void FlushUvs()
    {
        if (!uvsChanged) return;
        uvsChanged = false;
        mesh.uv = uvs;
        mesh.uv2 = blendUvs;
        mesh.uv3 = blendMaskUvs;
    }

    public void UpdatePosition(Int2 position)
    {
        x = position.x;
        y = position.y;
        transform.localPosition = new Vector3(x, y, 0);

        DoUpdatePosition(position);
    }

    protected abstract void DoUpdatePosition(Int2 position);

    public void LoadChunk(IntRect bounds)
    {
        if (IsOutOfBounds())
        {
            CheckEnabled();
            return;
        }
        DoLoadChunk(bounds);
        CheckEnabled();
    }

    protected abstract void DoLoadChunk(IntRect bounds);

    public void Dispose()
    {
        Destroy(gameObject);
    }
}
