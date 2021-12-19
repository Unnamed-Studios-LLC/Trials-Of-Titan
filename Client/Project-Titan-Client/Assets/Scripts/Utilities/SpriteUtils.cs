using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public static class SpriteUtils
{

    public static void SetAnchorRatio(RectTransform rectTransform, Sprite sprite)
    {
        var ratio = GetAnchorRatio(sprite);
        rectTransform.anchorMin = new Vector2((1 - ratio.x) / 2, (1 - ratio.y) / 2);
        rectTransform.anchorMax = new Vector2(1 - (1 - ratio.x) / 2, 1 - (1 - ratio.y) / 2);
        rectTransform.offsetMin = new Vector2(0, 0);
        rectTransform.offsetMax = new Vector2(0, 0);
    }

    public static Vector2 GetAnchorRatio(Sprite sprite)
    {
        var size = sprite.rect.size;
        if (size.x > size.y)
        {
            return new Vector2(1, size.y / size.x);
        }
        else
        {
            return new Vector2(size.x / size.y, 1);
        }
    }

    public static Vector2 GetDifferenceRatio(Vector2 a, Vector2 b)
    {
        return new Vector2(
            b.x / a.x,
            b.y / a.y
            );
    }

}
