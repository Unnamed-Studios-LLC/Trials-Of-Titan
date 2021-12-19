using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TitanCore.Data;
using TitanCore.Data.Components;
using TitanCore.Data.Map;
using TitanCore.Net;
using TitanCore.Net.Packets.Models;
using Utils.NET.Collections;
using Utils.NET.Geometry;
using Utils.NET.Logging;
using Utils.NET.Utils;
using WorldGen.Rasterization;
using static TitanCore.Files.MapElementFile;

namespace World.Map
{
    [Flags]
    public enum CollisionType
    {
        None = 0,
        Wall = 1,
        Object = 2,
        SightWall = 4
    }

    public class TileManager
    {
        private struct TileData
        {
            public MapTile tile;

            public float timeValue;

            public ulong tickId;

            public TileInfo info;

            public CollisionType collisionType;

            public TileData(MapTile tile)
            {
                this.tile = tile;
                timeValue = 0;
                tickId = 0;
                info = tile.GetTileInfo();

                if (tile.objectType == 0)
                    collisionType = CollisionType.None;
                else
                {
                    var objInfo = GameData.objects[tile.objectType];

                    collisionType = objInfo is WallInfo || objInfo is Object3dInfo ? CollisionType.Wall : ((objInfo is StaticObjectInfo staticObj && staticObj.collidable) ? CollisionType.Object : CollisionType.None);

                    if (objInfo is WallInfo || objInfo is Object3dInfo obj3dInfo && obj3dInfo.blockSight || (objInfo is StaticObjectInfo statO && statO.blockSight))
                    {
                        collisionType |= CollisionType.SightWall;
                    }
                }
            }
        }

        /// <summary>
        /// The world this tilemanager is in
        /// </summary>
        private World world;

        /// <summary>
        /// The width of the map
        /// </summary>
        private int width;

        /// <summary>
        /// The height of the map
        /// </summary>
        private int height;

        /// <summary>
        /// 2D grid array of all tiles within the map
        /// </summary>
        private TileData[,] tiles;

        /// <summary>
        /// The health of objects
        /// </summary>
        private Dictionary<uint, int> objectHealth = new Dictionary<uint, int>();

        public TileManager(World world, int width, int height, MapTileElement[,] elementTiles)
        {
            this.world = world;
            this.width = width + 2;
            this.height = height + 2;

            tiles = new TileData[width + 2, height + 2];
            for (uint y = 0; y < height; y++)
            {
                for (uint x = 0; x < width; x++)
                {
                    var tile = elementTiles[x, y];
                    tiles[x + 1, y + 1] = new TileData(new MapTile((ushort)(x + 1), (ushort)(y + 1), tile.tileType, tile.objectType));

                    if (tile.objectType > 0)
                    {
                        var info = GameData.objects[tile.objectType];
                        if (info is Object3dInfo object3dInfo)
                        {
                            if (object3dInfo.health > 0)
                            {
                                objectHealth.Add(GetWallKey(x + 1, y + 1), object3dInfo.health);
                            }
                        }
                    }
                }
            }
        }

        private uint GetWallKey(uint x, uint y)
        {
            return (x << 16) | y;
        }

        public void HitObject(uint x, uint y, int damage)
        {
            uint wallKey = GetWallKey(x, y);
            if (!objectHealth.TryGetValue(wallKey, out var wallHealth)) return;
            wallHealth -= damage;
            if (wallHealth < 0)
            {
                var data = tiles[x, y];
                var tile = data.tile;
                tile.objectType = 0;
                SetTileAndBroadcast(tile);
                objectHealth.Remove(wallKey);
            }
            else
                objectHealth[wallKey] = wallHealth;
        }

        public bool CanWalk(float fx, float fy)
        {
            int x = (int)fx, y = (int)fy;
            if (x < 0 || y < 0 || x >= width || y >= height) return false;

            var tileData = tiles[x, y];
            var tileInfo = tileData.tile.GetTileInfo();
            if (tileInfo == null) return false;
            if (tileInfo.noWalk) return false;

            var objInfo = tileData.tile.GetObjectInfo();
            if (objInfo != null)
            {
                if (objInfo is Object3dInfo obj3dInfo) return false;
                //if (objInfo is StaticObjectInfo staticObj && staticObj.collidable)
                //{
                //    return false;
                    //return !(new Vec2(fx, fy).DistanceTo(new Vec2(x + 0.5f, y + 0.5f)) <= 0.6f);
                //}
            }
            return true;
        }

        public bool PlayerCanWalk(float x, float y)
        {
            return CanWalkOn(x, y) && !PlayerCollides(x, y);
        }

        private bool CanWalkOn(float fx, float fy)
        {
            int x = (int)fx, y = (int)fy;
            if (x < 0 || y < 0 || x >= width || y >= height) return false;
            var tileData = tiles[x, y];
            if (tileData.tile.tileType == 0) return false;
            if (tileData.tile.GetTileInfo().noWalk) return false;
            return true;
        }

        private bool PlayerCollides(float x, float y)
        {
            var type = GetCollisionType(x, y);
            if (type.HasFlag(CollisionType.Wall)) return true;
            else if (type.HasFlag(CollisionType.Object) && ObjectCollides(x, y)) return true;

            if (GetCollisionType(x - NetConstants.Wall_Collision_Space, y + NetConstants.Wall_Collision_Space).HasFlag(CollisionType.Wall)) return true;
            if (GetCollisionType(x, y + NetConstants.Wall_Collision_Space).HasFlag(CollisionType.Wall)) return true;
            if (GetCollisionType(x + NetConstants.Wall_Collision_Space, y + NetConstants.Wall_Collision_Space).HasFlag(CollisionType.Wall)) return true;

            if (GetCollisionType(x - NetConstants.Wall_Collision_Space, y).HasFlag(CollisionType.Wall)) return true;
            if (GetCollisionType(x + NetConstants.Wall_Collision_Space, y).HasFlag(CollisionType.Wall)) return true;

            if (GetCollisionType(x - NetConstants.Wall_Collision_Space, y - NetConstants.Wall_Collision_Space).HasFlag(CollisionType.Wall)) return true;
            if (GetCollisionType(x, y - NetConstants.Wall_Collision_Space).HasFlag(CollisionType.Wall)) return true;
            if (GetCollisionType(x + NetConstants.Wall_Collision_Space, y - NetConstants.Wall_Collision_Space).HasFlag(CollisionType.Wall)) return true;

            return false;
        }

        private bool ObjectCollides(float x, float y)
        {
            return new Vec2(x, y).DistanceTo(new Vec2((int)x + 0.5f, (int)y + 0.5f)) <= 0.6f;
        }

        private bool OutOfBounds(int x, int y)
        {
            if (x < 0 || y < 0 || x >= width || y >= height) return true;
            return false;
        }

        public CollisionType GetCollisionType(float fx, float fy)
        {
            int x = (int)fx, y = (int)fy;
            if (OutOfBounds(x, y)) return CollisionType.Wall;
            return tiles[x, y].collisionType;
        }

        public MapTile GetTile(int x, int y)
        {
            return tiles[x, y].tile;
        }

        public void SetTile(MapTile tile)
        {
            tiles[tile.x, tile.y] = new TileData(tile);
        }

        public void SetTileAndBroadcast(MapTile tile)
        {
            SetTile(tile);

            foreach (var player in world.objects.players.Values)
                player.ResetTile(tile.x, tile.y);
        }

        public void UpdateTile(int x, int y, bool standingOn, ref WorldTime time)
        {
            var data = tiles[x, y];
            if (data.tile.tileType == 0) return;
            if (data.info.change == null) return;
            if (data.tickId == time.tickId) return;
            if (data.info.change.action == TileChangeAction.Pressure && !standingOn) return;
            if (time.tickId - data.tickId > 5)
            {
                data.timeValue = (float)time.totalTime + data.info.change.time;
            }

            data.tickId = time.tickId;
            //data.timeValue += (float)time.deltaTime;
            if (time.totalTime >= data.timeValue)
            {
                var tile = data.tile;
                tile.tileType = data.info.change.tile;
                SetTile(tile);

                foreach (var player in world.objects.players.Values)
                    player.ResetTile(x, y);
            }
            else
                tiles[x, y] = data;
        }
    }
}
