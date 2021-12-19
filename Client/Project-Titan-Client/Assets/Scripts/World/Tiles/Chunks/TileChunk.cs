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

public class TileChunk : GroundChunk<TileChunk>
{
    private enum BlendType
    {
        None = 0,
        Top = 1,
        Left = 2,
        Bottom = 4,
        Right = 8,

        TopLeft = Top | Left,
        TopRight = Top | Right,
        TopBottom = Top | Bottom,

        LeftBottom = Left | Bottom,
        LeftRight = Left | Right,

        BottomRight = Bottom | Right,

        TopLeftBottom = Top | Left | Bottom,
        LeftBottomRight = Left | Bottom | Right,
        BottomRightTop = Bottom | Right | Top,
        RightTopLeft = Right | Top | Left,

        All = Top | Left | Bottom | Right
    }

    private class TileData
    {
        public TileInfo tile;

        public TileInfo[] blends;

        private float frameTime;

        private int textureFrame = 0;

        public TileData()
        {
            blends = new TileInfo[4];
        }

        public void Clear()
        {
            for (int i = 0; i < blends.Length; i++)
                blends[i] = null;
        }

        public void UpdateBlend(TileInfo blend, Int2 direction)
        {
            int index = GetIndex(direction);
            blends[index] = blend;
        }

        public bool UpdateAnimation()
        {
            if (tile == null) return false;
            if (!tile.animated) return false;
            var frame = Mathf.RoundToInt(Time.time / tile.framesPerSecond);
            if (frame != textureFrame)
            {
                textureFrame = frame;
                return true;
            }
            return false;
        }

        public Sprite GetTileSprite()
        {
            string name = null;
            if (tile.animated && tile.selectionType != TileFrameSelectionType.Random)
            {
                switch (tile.selectionType)
                {
                    case TileFrameSelectionType.Sequence:
                        name = tile.textures[textureFrame % tile.textures.Length].displaySprite;
                    break;
                }
            }
            else
            {
                name = tile.textures[UnityEngine.Random.Range(0, tile.textures.Length)].displaySprite;
            }
            return TextureManager.GetSprite(name);
        }

        private int GetIndex(Int2 direction)
        {
            switch (direction.y)
            {
                case 1:
                    return 0;
                case 0:
                    return direction.x == -1 ? 1 : 3;
                default:
                    return 2;
            }
        }

        private TileInfo GetBlendTile()
        {
            int maxBlend = tile == null ? 0 : tile.blend;
            var blend = tile;
            for (int i = 0; i < 4; i++)
            {
                var t = blends[i];
                if (t == null) continue;
                if (t.blend > maxBlend)
                {
                    maxBlend = t.blend;
                    blend = t;
                }
            }
            return blend;
        }

        public Sprite GetMask(out TileRotation rotation, out Sprite blendSprite)
        {
            if (tile != null && tile.blend == -1)
            {
                rotation = TileRotation.None;
                blendSprite = null;
                return null;
            }

            int tileBlend = tile?.blend ?? 0;

            var blendTile = GetBlendTile();
            BlendType blendType = 0;

            if (blendTile == null || blendTile == tile || tileBlend > blendTile.blend)
            {
                rotation = TileRotation.None;
                blendSprite = null;
                return null;
            }

            for (int i = 0; i < 4; i++)
            {
                if (blends[i] == blendTile)
                    blendType |= (BlendType)(1 << i);
            }

            blendSprite = TextureManager.GetDisplaySprite(blendTile);

            Sprite mask;
            switch (blendType)
            {
                case BlendType.Top:
                    rotation = TileRotation.Half;
                    mask = TextureManager.GetSprite("Blend 1");
                    break;
                case BlendType.Left:
                    rotation = TileRotation.ThreeQuarters;
                    mask = TextureManager.GetSprite("Blend 1");
                    break;
                case BlendType.Right:
                    rotation = TileRotation.Quarter;
                    mask = TextureManager.GetSprite("Blend 1");
                    break;
                case BlendType.Bottom:
                    rotation = TileRotation.None;
                    mask = TextureManager.GetSprite("Blend 1");
                    break;

                case BlendType.TopLeft:
                    rotation = TileRotation.ThreeQuarters;
                    mask = TextureManager.GetSprite("Blend 2");
                    break;
                case BlendType.TopBottom:
                    rotation = TileRotation.None;
                    mask = TextureManager.GetSprite("Blend 3");
                    break;
                case BlendType.TopRight:
                    rotation = TileRotation.Half;
                    mask = TextureManager.GetSprite("Blend 2");
                    break;

                case BlendType.LeftBottom:
                    rotation = TileRotation.None;
                    mask = TextureManager.GetSprite("Blend 2");
                    break;
                case BlendType.LeftRight:
                    rotation = TileRotation.Quarter;
                    mask = TextureManager.GetSprite("Blend 3");
                    break;

                case BlendType.BottomRight:
                    rotation = TileRotation.Quarter;
                    mask = TextureManager.GetSprite("Blend 2");
                    break;

                case BlendType.TopLeftBottom:
                    rotation = TileRotation.Half;
                    mask = TextureManager.GetSprite("Blend 4");
                    break;
                case BlendType.LeftBottomRight:
                    rotation = TileRotation.ThreeQuarters;
                    mask = TextureManager.GetSprite("Blend 4");
                    break;
                case BlendType.BottomRightTop:
                    rotation = TileRotation.None;
                    mask = TextureManager.GetSprite("Blend 4");
                    break;
                case BlendType.RightTopLeft:
                    rotation = TileRotation.Quarter;
                    mask = TextureManager.GetSprite("Blend 4");
                    break;

                case BlendType.All:
                    rotation = TileRotation.None;
                    mask = TextureManager.GetSprite("Blend 5");
                    break;

                default:
                    rotation = TileRotation.None;
                    mask = null;
                    break;
            }
            return mask;
        }
    }

    private Int2 position;

    private TileData[,] tileDatas;

    private Int2 size;

    public override void Initialize(TilemapManager manager, int chunkSize, int sortOrder)
    {
        base.Initialize(manager, chunkSize, sortOrder);

        size = new Int2(chunkSize, chunkSize);

        tileDatas = new TileData[chunkSize, chunkSize];
        for (int y = 0; y < chunkSize; y++)
            for (int x = 0; x < chunkSize; x++)
                tileDatas[x, y] = new TileData();
    }

    protected override void DoUpdatePosition(Int2 position)
    {
        this.position = position;
    }

    protected override void DoLoadChunk(IntRect bounds)
    {
        for (int y = bounds.y; y < bounds.y + bounds.height; y++)
            for (int x = bounds.x; x < bounds.x + bounds.width; x++)
            {
                tileDatas[x - position.x, y - position.y].Clear();
            }

        for (int y = bounds.y; y < bounds.y + bounds.height; y++)
            for (int x = bounds.x; x < bounds.x + bounds.width; x++)
            {
                manager.SetLiveTile(x, y, this, manager.GetInfo(manager.GetTileType(x, y)));
            }
    }

    public override void LateUpdate()
    {
        base.LateUpdate();

        for (int y = 0; y < chunkSize; y++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                var d = tileDatas[x, y];
                if (d == null) continue;
                if (!d.UpdateAnimation()) continue;
                ApplySprite(x, y, d.GetTileSprite());
            }
        }
    }

    public void SetTile(int x, int y, TileInfo info, Sprite sprite)
    {
        int localX = x - position.x;
        int localY = y - position.y;

        if (info == null)
        {
            tileDatas[localX, localY].tile = null;
            RemoveSprite(localX, localY);
            return;
        }

        tileDatas[localX, localY].tile = info;
        ApplySprite(localX, localY, sprite, TileRotation.None);
    }

    public void UpdateBlend(int x, int y, TileInfo blend, Int2 direction)
    {
        int localX = x - position.x;
        int localY = y - position.y;
        if (localX < 0 || localX >= size.x || localY < 0 || localY >= size.y) return;
        var blendData = tileDatas[localX, localY];
        blendData.UpdateBlend(blend, direction);
        TileRotation maskRotation;
        Sprite blendSprite;
        var mask = blendData.GetMask(out maskRotation, out blendSprite);

        if (mask == null)
            RemoveBlend(localX, localY);
        else
            ApplyBlend(x - position.x, y - position.y, blendSprite, mask, TileRotation.None, maskRotation);
    }

    public void RemoveBlend(int x, int y, Int2 direction)
    {
        int localX = x - position.x;
        int localY = y - position.y;
        if (localX < 0 || localX >= size.x || localY < 0 || localY >= size.y) return;

        var blendData = tileDatas[localX, localY];
        blendData.UpdateBlend(null, direction);
        TileRotation maskRotation;
        Sprite blendSprite;
        var mask = blendData.GetMask(out maskRotation, out blendSprite);

        if (mask == null)
            RemoveBlend(localX, localY);
        else
            ApplyBlend(x - position.x, y - position.y, blendSprite, mask, TileRotation.None, maskRotation);
    }
}