using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class FollowColor : MonoBehaviour
{
    public Graphic following;

    public Graphic follower;

    public bool r = true;

    public bool g = true;

    public bool b = true;

    public bool a = true;

    private void LateUpdate()
    {
        if (following == null || follower == null) return;

        var color = GetColor(following);
        var oldColor = GetColor(follower);
        SetColor(follower, new Color(r ? color.r : oldColor.r, g ? color.g : oldColor.g, b ? color.b : oldColor.b, a ? color.a : oldColor.a));
    }

    private Color GetColor(Graphic component)
    {
        switch (component)
        {
            case TextMeshProUGUI label:
                return label.color;
            case Image image:
                return image.color;
        }
        return Color.white;
    }

    private void SetColor(Graphic component, Color color)
    {
        switch (component)
        {
            case TextMeshProUGUI label:
                label.color = color;
                break;
            case Image image:
                image.color = color;
                break;
        }
    }
}
