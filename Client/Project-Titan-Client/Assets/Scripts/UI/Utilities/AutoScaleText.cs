using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

[ExecuteInEditMode]
public class AutoScaleText : MonoBehaviour
{
    public TextMeshProUGUI label;

    public float scale = 0.27777777f;

    private void LateUpdate()
    {
        if (label == null) return;
        var height = label.rectTransform.rect.height;
        var size = height * scale;
        label.fontSize = size;
    }
}
