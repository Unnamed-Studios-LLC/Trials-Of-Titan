using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionPageScaler : MonoBehaviour
{
    public RectTransform parent;

    public RectTransform target;

    public RectTransform content;

    public float targetHeight;

    private void Start()
    {
        var targetSize = parent.rect.height * targetHeight;
        target.sizeDelta = new Vector2(target.sizeDelta.x, targetSize);
        target.ForceUpdateRectTransforms();

        var targetRect = target.rect;

        float yMax = target.position.y - target.pivot.y * targetRect.height + targetRect.height;
        float yMin = 9999999;

        foreach (RectTransform child in target.GetComponentInChildren<RectTransform>(true))
        {
            child.ForceUpdateRectTransforms();

            var childRect = child.rect;
            yMax = Mathf.Max(yMax, child.position.y - child.pivot.y * childRect.height + childRect.height);
            yMin = Mathf.Min(yMin, child.position.y - child.pivot.y * childRect.height);
        }

        content.sizeDelta = new Vector2(content.sizeDelta.x, yMax - yMin + targetSize);
    }
}
