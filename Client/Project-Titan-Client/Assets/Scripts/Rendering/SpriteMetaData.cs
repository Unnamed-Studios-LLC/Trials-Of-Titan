using System;
using System.Collections.Generic;
using UnityEngine;
using Utils.NET.Utils;

public class SpriteMetaData
{
    public Color[] colors;

    public Color averageColor;

    public Color modeColor;

    public SpriteMetaData(Sprite sprite)
    {
        var rect = sprite.textureRect;
        var pixels = sprite.texture.GetPixels((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);
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
        colors = colorList.ToArray();

        averageColor = new Color(r / colors.Length, g / colors.Length, b / colors.Length, 1);
        modeColor = modeCount.Closest(_ => -_.Value).Key;
    }
}
