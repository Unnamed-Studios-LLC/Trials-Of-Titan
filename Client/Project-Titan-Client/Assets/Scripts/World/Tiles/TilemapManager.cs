using UnityEngine;
using System.Collections;
using Utils.NET.Partitioning;
using Utils.NET.Geometry;
using TitanCore.Net.Packets.Models;
using TitanCore.Data.Map;
using TitanCore.Data;

public class TilemapManager : MonoBehaviour
{
    [System.Flags]
    private enum TileShadowType
    {
        None = 0,

        Down = 1,
        Right = 2,
        Up = 4,
        Left = 8,

        DownRight = 3,
        RightUp = 6,
        UpLeft = 12,
        LeftDown = 9,

        DownUp = 5,
        RightLeft = 10,

        DownRightUp = 7,
        RightUpLeft = 14,
        UpLeftDown = 13,
        LeftDownRight = 11,

        DownRightUpLeft = 15
    }

    public int width;

    public int height;

    public World world;

    public int chunkSize;

    public GameObject tileChunkPrefab;

    private ChunkMap<TileChunk> tiles;

    private ChunkMap<ObjectChunk> objects;

    private ushort[,] tilesTypes;

    private ushort[,] objectTypes;

    public void Initialize(int width, int height)
    {
        if (tiles != null)
        {
            tiles.Dispose();
        }

        if (objects != null)
        {
            objects.Dispose();
        }

        this.width = width;
        this.height = height;

        tilesTypes = new ushort[width, height];
        objectTypes = new ushort[width, height];

        float viewHeight = world.worldCamera.orthographicSize * 2;
        float viewWidth = world.worldCamera.aspect * viewHeight;

        var viewportWidth = Mathf.CeilToInt(viewWidth) + chunkSize * 2;
        var viewportHeight = Mathf.CeilToInt(viewHeight) + chunkSize * 2;

        var viewportSize = new Int2(viewportWidth, viewportHeight);

        tiles = new ChunkMap<TileChunk>(width, height, chunkSize, viewportSize, CreateTileChunk);
        objects = new ChunkMap<ObjectChunk>(width, height, chunkSize, viewportSize, CreateObjectChunk);
    }

    private TileChunk CreateTileChunk(Int2 position, int chunkSize)
    {
        var chunk = Instantiate(tileChunkPrefab).GetComponent<TileChunk>();
        chunk.transform.SetParent(transform);
        chunk.Initialize(this, chunkSize, 0);
        chunk.transform.localPosition = new Vector3(position.x, position.y, 0);
        chunk.transform.localEulerAngles = Vector3.zero;
        return chunk;
    }

    private ObjectChunk CreateObjectChunk(Int2 position, int chunkSize)
    {
        var chunk = new ObjectChunk(this);
        return chunk;
    }

    public void ProcessMapTile(MapTile tile)
    {
        objectTypes[tile.x, tile.y] = tile.objectType;

        WorldObject obj = null;
        var objectChunk = objects.LiveChunkAt(tile.x, tile.y);
        if (objectChunk != null)
        {
            obj = objectChunk.SetObject(tile.x, tile.y, tile.objectType);
            if (obj is Wall)
            {
                tile.tileType = 0;
            }
        }

        SetTileType(tile.x, tile.y, tile.tileType);
        SetLiveTile(tile.x, tile.y, GetInfo(tile.tileType));

        world.gameManager.ui.TileDiscovered(tile);
    }

    public void SetLiveTile(int x, int y, TileInfo info)
    {
        var tileChunk = tiles.LiveChunkAt(x, y);
        if (tileChunk == null) return;
        SetLiveTile(x, y, tileChunk, info);
    }

    public void SetLiveTile(int x, int y, TileChunk tileChunk, TileInfo info)
    {
        if (info != null)
        {
            var texture = info.textures[UnityEngine.Random.Range(0, info.textures.Length)];
            var sprite = TextureManager.GetSprite(texture.displaySprite);

            tileChunk.SetTile(x, y, info, sprite);
        }
        else
        {
            tileChunk.SetTile(x, y, null, null);
        }

        DoBlendCalc(x, y, info);
    }

    private void DoBlendCalc(int x, int y, TileInfo info)
    {
        int bx = 1;
        for (int by = 0; by < 3; by++)
        {
            for (bx = bx % 3; bx < 3; bx += 2)
            {
                var pos = new Int2(x + bx - 1, y + by - 1);
                if (pos.x < 0 || pos.y < 0 || pos.x >= width || pos.y >= height) continue;
                if (info == null)
                    RemoveBlend(pos.x, pos.y, new Int2(-(bx - 1), -(by - 1)));
                else
                    UpdateBlend(pos.x, pos.y, info, new Int2(-(bx - 1), -(by - 1)));

                var t = GetInfo(GetTileType(pos.x, pos.y));
                if (t == null)
                    RemoveBlend(x, y, new Int2(bx - 1, by - 1));
                else
                    UpdateBlend(x, y, t, new Int2(bx - 1, by - 1));
            }
        }
    }

    public WorldObject GetObject(int x, int y)
    {
        if (x < 0 || y < 0 || x >= width || y >= height) return null;
        var objectChunk = objects.LiveChunkAt(x, y);
        if (objectChunk == null) return null;
        return objectChunk.GetObject(x, y);
    }

    private void UpdateBlend(int x, int y, TileInfo blendType, Int2 blendDirection)
    {
        var tileChunk = tiles.LiveChunkAt(x, y);
        if (tileChunk == null) return;
        tileChunk.UpdateBlend(x, y, blendType, blendDirection);
    }

    private void RemoveBlend(int x, int y, Int2 blendDirection)
    {
        var tileChunk = tiles.LiveChunkAt(x, y);
        if (tileChunk == null) return;
        tileChunk.RemoveBlend(x, y, blendDirection);
    }

    public TileInfo GetInfo(ushort type)
    {
        if (type == 0) return null;
        return (TileInfo)GameData.objects[type];
    }

    public void SetFocus(int x, int y)
    {
        tiles.SetFocus(x, y);
        objects.SetFocus(x, y);
    }

    public bool CanWalkOn(float fx, float fy)
    {
        int x = (int)fx, y = (int)fy;
        if (x < 0 || y < 0 || x >= width || y >= height) return false;
        var tile = tilesTypes[x, y];
        if (tile == 0) return false;
        return !GetInfo(tile).noWalk;
    }

    public ushort GetTileType(int x, int y)
    {
        if (x < 0 || y < 0 || x >= width || y >= height) return 0;
        return tilesTypes[x, y];
    }

    public ushort GetObjectType(int x, int y)
    {
        if (x < 0 || y < 0 || x >= width || y >= height) return 0;
        return objectTypes[x, y];
    }

    private void SetTileType(int x, int y, ushort type)
    {
        tilesTypes[x, y] = type;

        GetWall(x + 1, y)?.UpdateVision(MeshFace.Left, type != 0);
        GetWall(x - 1, y)?.UpdateVision(MeshFace.Right, type != 0);
        GetWall(x, y + 1)?.UpdateVision(MeshFace.Down, type != 0);
        GetWall(x, y - 1)?.UpdateVision(MeshFace.Up, type != 0);
    }

    public void WallAdded(int x, int y, Wall wall)
    {
        wall.UpdateVision(MeshFace.Down, GetTileType(x, y - 1) != 0);
        wall.UpdateVision(MeshFace.Up, GetTileType(x, y + 1) != 0);
        wall.UpdateVision(MeshFace.Left, GetTileType(x - 1, y) != 0);
        wall.UpdateVision(MeshFace.Right, GetTileType(x + 1, y) != 0);
    }

    private Wall GetWall(int x, int y)
    {
        var objectChunk = objects.LiveChunkAt(x, y);
        if (objectChunk != null)
            return objectChunk.GetObject(x, y) as Wall;
        return null;
    }
}
