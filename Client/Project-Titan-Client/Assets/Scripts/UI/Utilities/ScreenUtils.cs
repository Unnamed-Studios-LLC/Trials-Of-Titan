using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Utils.NET.Geometry;

public class ScreenUtils
{
    public static bool ScreenChangedSize(ref Int2 storedSize)
    {
        if (Screen.width == storedSize.x && Screen.height == storedSize.y) return false;
        storedSize = new Int2(Screen.width, Screen.height);
        return true;
    }
}
