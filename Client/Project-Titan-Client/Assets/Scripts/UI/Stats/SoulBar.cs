using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class SoulBar : MonoBehaviour
{
    public TextMeshProUGUI label;

    public RectTransform valueBar;

    public RectTransform containerBar;

    public void SetSouls(int value, int goal)
    {
        if (goal == 0)
        {
            label.text = $"{value}";
            Resize(1);
        }
        else
        {
            label.text = $"{value} / {goal}";
            Resize(value / (float)goal);
        }
    }

    private void Resize(float value)
    {
        valueBar.anchorMax = new Vector2(value, 1);
        valueBar.offsetMax = new Vector2(0, 0);
    }
}
