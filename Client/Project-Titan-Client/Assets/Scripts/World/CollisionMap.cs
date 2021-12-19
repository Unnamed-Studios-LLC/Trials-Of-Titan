using System.Collections;
using System.Collections.Generic;
using TitanCore.Net;
using UnityEngine;
using Utils.NET.Geometry;

public enum CollisionType : byte
{
    None,
    Wall,
    Object
}

public class CollisionMap
{

    /// <summary>
    /// The width of the map in units
    /// </summary>
    private int width;

    /// <summary>
    /// The height of the map in units
    /// </summary>
    private int height;

    /// <summary>
    /// Grid array containing collision data for each grid unit
    /// </summary>
    private CollisionType[,] map;

    public CollisionMap(int width, int height)
    {
        this.width = width;
        this.height = height;

        map = new CollisionType[width, height];
    }

    public void Set(int x, int y, CollisionType type)
    {
        map[x, y] = type;
    }

    private CollisionType GetType(float x, float y)
    {
        if (x < 0) x -= 1;
        if (y < 0) y -= 1;
        return GetType((int)x, (int)y);
    }

    private CollisionType GetType(int x, int y)
    {
        if (x < 0 || y < 0 || x >= width || y >= height) return CollisionType.Wall;
        return map[x, y];
    }

    public bool ProjectileCollides(float x, float y)
    {
        if (x < 0 || y < 0 || x >= width || y >= height) return false;
        var type = map[(int)x, (int)y];
        if (type == CollisionType.Wall) return true;
        else if (type == CollisionType.Object) return ObjectCollides(x, y);
        return false;
    }

    public bool PlayerCollides(float x, float y)
    {
        var type = GetType(x, y);
        if (type == CollisionType.Wall) return true;
        else if (type == CollisionType.Object && ObjectCollides(x, y)) return true;

        if (GetType(x - NetConstants.Wall_Collision_Space, y + NetConstants.Wall_Collision_Space) == CollisionType.Wall) return true;
        if (GetType(x, y + NetConstants.Wall_Collision_Space) == CollisionType.Wall) return true;
        if (GetType(x + NetConstants.Wall_Collision_Space, y + NetConstants.Wall_Collision_Space) == CollisionType.Wall) return true;

        if (GetType(x - NetConstants.Wall_Collision_Space, y) == CollisionType.Wall) return true;
        if (GetType(x + NetConstants.Wall_Collision_Space, y) == CollisionType.Wall) return true;

        if (GetType(x - NetConstants.Wall_Collision_Space, y - NetConstants.Wall_Collision_Space) == CollisionType.Wall) return true;
        if (GetType(x, y - NetConstants.Wall_Collision_Space) == CollisionType.Wall) return true;
        if (GetType(x + NetConstants.Wall_Collision_Space, y - NetConstants.Wall_Collision_Space) == CollisionType.Wall) return true;

        return false;
    }

    public bool IsWall(float x, float y)
    {
        return GetType(x, y) == CollisionType.Wall;
    }

    private bool ObjectCollides(float x, float y)
    {
        //var dif = new Vec2((int)x + 0.5f, (int)y + 0.5f).Subtract(new Vec2(x, y));
        //return Mathf.Abs(dif.x) < 0.4f || Mathf.Abs(dif.y) < 0.4f;
        return new Vec2(x, y).DistanceTo(new Vec2((int)x + 0.5f, (int)y + 0.5f)) <= 0.6f;
    }
}
