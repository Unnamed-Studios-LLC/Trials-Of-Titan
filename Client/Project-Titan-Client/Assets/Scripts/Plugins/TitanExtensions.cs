using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Core;
using UnityEngine;
using Utils.NET.Geometry;

public static class TitanExtensions
{
    /// <summary>
    /// Converts a Vec2 to Unity's Vector2 structure
    /// </summary>
    /// <param name="vec"></param>
    /// <returns></returns>
    public static Vector2 ToVector2(this Vec2 vec) => new Vector2(vec.x, vec.y);

    /// <summary>
    /// Converts a Vector2 to Vec2 structure
    /// </summary>
    /// <param name="vec"></param>
    /// <returns></returns>
    public static Vec2 ToVec2(this Vector2 vec) => new Vec2(vec.x, vec.y);


    /// <summary>
    /// Converts a GameColor to a Unity Color
    /// </summary>
    /// <param name="color"></param>
    /// <returns></returns>
    public static Color ToUnityColor(this GameColor gameColor) => new Color((gameColor.r + 128f) / 255f, (gameColor.g + 128f) / 255f, (gameColor.b + 128f) / 255f, 1);

    /// <summary>
    /// Converts a Unity Color to a Titan GameColor
    /// </summary>
    /// <param name="color"></param>
    /// <returns></returns>
    public static GameColor ToGameColor(this Color color) => new GameColor((sbyte)(-128 + (int)(255 * color.r)), (sbyte)(-128 + (int)(255 * color.g)), (sbyte)(-128 + (int)(255 * color.b)));
}