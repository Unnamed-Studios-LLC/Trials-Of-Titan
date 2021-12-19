using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class BarStat : Stat
{
    private int max;

    private int value;

    public RectTransform valueBar;

    public RectTransform containerBar;

    public override void SetStat(int value, int extra, Player player)
    {
        max = value + extra;
        CheckMax(value, player);
        Resize();
    }

    public void SetValue(int value)
    {
        this.value = value;
        stat.text = value.ToString();
        Resize();
    }

    private void Resize()
    {
        valueBar.anchorMax = new Vector2((float)value / max, 1);
        valueBar.offsetMax = new Vector2(0, 0);
    }
}