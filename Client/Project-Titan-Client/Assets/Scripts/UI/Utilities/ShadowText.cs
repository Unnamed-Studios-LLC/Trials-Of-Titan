using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[ExecuteInEditMode]
public class ShadowText : MonoBehaviour
{
    public TextMeshProUGUI target;

    public bool followFontSize = false;

    public bool followColor = true;

    private TextMeshProUGUI self;

    private void Awake()
    {
        self = GetComponent<TextMeshProUGUI>();
    }

    private void LateUpdate()
    {
        if (target == null) return;

        self.text = target.text;
        if (followFontSize)
            self.fontSize = target.fontSize;
        if (followColor)
            self.color = target.color;
    }
}
