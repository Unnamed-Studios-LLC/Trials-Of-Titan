using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

[ExecuteInEditMode]
public class SizeToLabel : MonoBehaviour
{
    public TextMeshProUGUI label;

    public Vector2 offset;

    private RectTransform rect;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    private void LateUpdate()
    {
        var bounds = label.textBounds;
        rect.sizeDelta = new Vector2(bounds.size.x, bounds.size.y) + offset;
    }
}
