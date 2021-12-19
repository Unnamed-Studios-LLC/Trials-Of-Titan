using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MinMaxOffset : MonoBehaviour
{
    public enum FollowType
    {
        WidthFollowHeight,
        HeightFollowWidth
    }

    public FollowType followType;

    public bool overrideFollowMinMax;

    public Vector2 followMinMax;

    private RectTransform rectTransform;

    private Rect rect;

    private void OnEnable()
    {
        rectTransform = GetComponent<RectTransform>();
        Size(false);
    }

    // Update is called once per frame
    private void LateUpdate()
    {
        Size(true);
    }

    private void Size(bool onChange)
    {
        //if (!onChange)
        //    rectTransform.ForceUpdateRectTransforms();

        var newRect = ((RectTransform)rectTransform.parent).rect;
        if (newRect == rect && onChange) return;
        rect = newRect;

        var min = rectTransform.anchorMin;
        var max = rectTransform.anchorMax;

        var minOffset = rectTransform.offsetMin;
        var maxOffset = rectTransform.offsetMax;

        var followMinMax = Vector2.zero;
        var followSize = Vector2.zero;
        float followDimensionSize = 0;
        var followerAnchors = Vector2.zero;
        switch (followType)
        {
            case FollowType.HeightFollowWidth:
                followMinMax = overrideFollowMinMax ? this.followMinMax : new Vector2(min.x, max.x);
                followSize = new Vector2(rect.width * followMinMax.x, rect.width - (rect.width * followMinMax.y));
                followDimensionSize = rect.height;
                break;
            case FollowType.WidthFollowHeight:
                followMinMax = overrideFollowMinMax ? this.followMinMax : new Vector2(min.y, max.y);
                followSize = new Vector2(rect.height * followMinMax.x, rect.height - (rect.height * followMinMax.y));
                followDimensionSize = rect.width;
                break;
        }

        //followSize = new Vector2(rect.width * followMinMax.x, rect.width - (rect.width * followMinMax.y));
        followerAnchors = new Vector2(followSize.x / followDimensionSize, (followDimensionSize - followSize.y) / followDimensionSize);

        switch (followType)
        {
            case FollowType.HeightFollowWidth:
                rectTransform.anchorMin = new Vector2(min.x, followerAnchors.x);
                rectTransform.anchorMax = new Vector2(max.x, followerAnchors.y);

                break;
            case FollowType.WidthFollowHeight:
                rectTransform.anchorMin = new Vector2(followerAnchors.x, min.y);
                rectTransform.anchorMax = new Vector2(followerAnchors.y, max.y);
                break;
        }

        rectTransform.offsetMin = minOffset;
        rectTransform.offsetMax = maxOffset;

        rectTransform.ForceUpdateRectTransforms();
        rect = rectTransform.rect;
    }
}
