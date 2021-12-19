using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum SpriteFillType
{
    Fill,
    FitHeight,
    HitWidth
}

[ExecuteInEditMode]
public class SpriteFrame : MonoBehaviour
{
    public Image spriteRenderer;

    public Vector2 anchor = new Vector2(0.5f, 0.5f);

    public ScaleMode scaleMode = ScaleMode.ScaleToFit;

    private void OnEnable()
    {
        Position();
    }

    private void Position()
    {
        if (spriteRenderer == null) return;

        var rectTransform = (RectTransform)transform;
        var spriteTransform = (RectTransform)spriteRenderer.transform;

        var sprite = spriteRenderer.sprite;
        if (sprite == null) return;

        var spriteSize = sprite.rect.size;
        var rectSize = rectTransform.rect.size;
        var differenceRatio = rectSize / spriteSize;

        Vector2 frameSize;
        switch (scaleMode)
        {
            case ScaleMode.ScaleAndCrop:
                frameSize = spriteSize * Mathf.Max(differenceRatio.x, differenceRatio.y);
                break;
            case ScaleMode.ScaleToFit:
                frameSize = spriteSize * Mathf.Min(differenceRatio.x, differenceRatio.y);
                break;
            default:
                frameSize = rectSize;
                break;
        }

        spriteTransform.anchorMin = spriteTransform.anchorMax = anchor;
        spriteTransform.pivot = anchor;
        spriteTransform.sizeDelta = frameSize;
        spriteTransform.anchoredPosition = Vector2.zero;
    }
}
