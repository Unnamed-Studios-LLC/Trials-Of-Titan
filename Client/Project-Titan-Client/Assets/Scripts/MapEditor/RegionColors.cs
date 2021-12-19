using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Core;
using UnityEngine;

public static class RegionColors
{
    public static Color Get(Region region)
    {
        switch (region)
        {
            case Region.Spawn:
                return Color.green;
            case Region.Village:
                return LightenColor(Color.green, 0.4f);
            case Region.Portal:
                return Color.cyan;
            case Region.Shop1:
                return Color.magenta;
            case Region.Shop2:
                return Color.magenta;
            case Region.Shop3:
                return Color.magenta;
            case Region.Shop4:
                return Color.magenta;
            case Region.Tag1:
            case Region.Tag2:
            case Region.Tag3:
            case Region.Tag4:
            case Region.Tag5:
            case Region.Tag6:
            case Region.Tag7:
            case Region.Tag8:
            case Region.Tag9:
            case Region.Tag10:
            case Region.Tag11:
            case Region.Tag12:
            case Region.Tag13:
            case Region.Tag14:
            case Region.Tag15:
            case Region.Tag16:
                return Color.yellow;
            default:
                return Color.blue;
        }
    }

    private static Color LightenColor(Color color, float amount)
    {
        return new Color(Lighten(color.r, amount), Lighten(color.g, amount), Lighten(color.b, amount), color.a);
    }

    private static float Lighten(float value, float amount)
    {
        return value + (1 - value) * amount;
    }
}
