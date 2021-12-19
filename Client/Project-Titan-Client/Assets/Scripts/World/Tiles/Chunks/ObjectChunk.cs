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

public class ObjectChunk : IChunk
{
    private TilemapManager manager;

    private WorldObject[,] objects;

    private Int2 position;

    public ObjectChunk(TilemapManager manager)
    {
        this.manager = manager;
    }

    public void UpdatePosition(Int2 position)
    {
        this.position = position;
    }

    public void LoadChunk(IntRect bounds)
    {
        ClearObjects();

        objects = new WorldObject[bounds.width, bounds.height];
        for (int y = 0; y < bounds.height; y++)
            for (int x = 0; x < bounds.width; x++)
            {
                int bx = bounds.x + x;
                int by = bounds.y + y;
                if (bx < 0 || by < 0 || bx >= manager.width || by >= manager.height) continue;
                SetObjectLocal(x, y, manager.GetObjectType(bx, by));
            }
    }

    private void ClearObjects()
    {
        if (objects == null) return;
        int width = objects.GetLength(0);
        int height = objects.GetLength(1);
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                var obj = objects[x, y];
                if (obj == null) continue;
                if (obj.info is Object3dInfo obj3dInfo && obj3dInfo.health > 0)
                {
                    manager.world.hittables.Remove(obj);
                }
                manager.world.gameManager.objectManager.ReturnObject(obj);
            }
        objects = null;
    }

    public WorldObject GetObject(int x, int y)
    {
        return GetObjectLocal(x - position.x, y - position.y);
    }

    private WorldObject GetObjectLocal(int x, int y)
    {
        if (x < 0 || y < 0) return null;
        return objects[x, y];
    }

    public WorldObject SetObject(int x, int y, ushort type)
    {
        return SetObjectLocal(x - position.x, y - position.y, type);
    }

    private WorldObject SetObjectLocal(int x, int y, ushort type)
    {
        var cur = objects[x, y];
        if (cur != null)
        {
            RemoveCollisionLocal(x, y, cur);

            if (cur.info is Object3dInfo obj3dInfo && obj3dInfo.health > 0)
            {
                manager.world.hittables.Remove(cur);
            }
            manager.world.gameManager.objectManager.ReturnObject(cur);
            objects[x, y] = null;
        }

        if (type == 0) return null;

        if (!manager.world.gameManager.objectManager.TryGetObject(type, out var obj))
        {
            Debug.Log("Failed to create object of type: 0x" + type.ToString("X"));
            return null;
        }

        if (obj is SpriteWorldObject && !(obj is GroundObject))
            obj.SetPosition(new Vec2(position.x + x + 0.5f + UnityEngine.Random.Range(-0.003f, 0.003f), position.y + y + 0.5f + UnityEngine.Random.Range(-0.003f, 0.003f)), true);
        else
            obj.SetPosition(new Vec2(position.x + x, position.y + y), true);
        objects[x, y] = obj;
        obj.gameId = uint.MaxValue;

        if (obj is Wall wall)
            manager.WallAdded(position.x + x, position.y + y, wall);

        if (obj.info is Object3dInfo object3dInfo && object3dInfo.health > 0)
        {
            manager.world.hittables.Add(obj);
        }

        AddCollisionLocal(x, y, obj);

        return obj;
    }

    private void RemoveCollisionLocal(int x, int y, WorldObject obj)
    {
        if (obj.ObjectType == GameObjectType.Wall || obj.ObjectType == GameObjectType.Object3d || obj.ObjectType == GameObjectType.ContextualWall)
        {
            manager.world.collision.Set(position.x + x, position.y + y, CollisionType.None);
        }
        else if (obj is StaticObject staticObject)
        {
            var staticInfo = (StaticObjectInfo)staticObject.info;
            if (staticInfo.blockSight)
            {
                manager.world.collision.Set(position.x + x, position.y + y, CollisionType.None);
            }
            else if (staticInfo.collidable)
            {
                manager.world.collision.Set(position.x + x, position.y + y, CollisionType.None);
            }
        }
    }

    private void AddCollisionLocal(int x, int y, WorldObject obj)
    {
        if (obj.ObjectType == GameObjectType.Wall || obj.ObjectType == GameObjectType.Object3d || obj.ObjectType == GameObjectType.ContextualWall)
        {
            manager.world.collision.Set(position.x + x, position.y + y, CollisionType.Wall);
        }
        else if (obj is StaticObject staticObject)
        {
            var staticInfo = (StaticObjectInfo)staticObject.info;
            if (staticInfo.blockSight)
            {
                manager.world.collision.Set(position.x + x, position.y + y, CollisionType.Wall);
            }
            else if (staticInfo.collidable)
            {
                manager.world.collision.Set(position.x + x, position.y + y, CollisionType.Object);
            }
        }
    }

    public void Dispose()
    {
        ClearObjects();
    }
}
