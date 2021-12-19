using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils.NET.Geometry;

public class GameViewScaler : MonoBehaviour
{
    private RectTransform rectTransform;

    private Int2 storedSize = new Int2(0, 0);

    private void OnEnable()
    {
        rectTransform = GetComponent<RectTransform>();
        Size();
    }

    private void LateUpdate()
    {
        if (ScreenUtils.ScreenChangedSize(ref storedSize))
            Size();
    }

    private void Size()
    {
        rectTransform.sizeDelta = new Vector2(Screen.width - Screen.height * 0.25f, Screen.height);
    }
}
