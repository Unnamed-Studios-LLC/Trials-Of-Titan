using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class RectLayout : MonoBehaviour
{
    public int slotsPerRow;

    public RectTransform[] rects;

    public RectTransform content;

    private void OnEnable()
    {
        LayoutRects();
    }

    private void LayoutRects()
    {
        float width = content.rect.width;
        for (int i = 0; i < rects.Length; i++)
        {
            var rect = GetSlotRect(i);
            var rectTransform = rects[i];
            rectTransform.anchoredPosition = rect.position * width;
            rectTransform.sizeDelta = rect.size * width;

            var fadeIn = rectTransform.GetComponent<FadeInMoveUp>();
            if (fadeIn != null)
                fadeIn.Run();
        }

        var contentSize = GetSlotRect(rects.Length - 1);
        content.sizeDelta = new Vector2(content.sizeDelta.x, (-contentSize.position.y + contentSize.size.y) * width);
    }

    private Rect GetSlotRect(int index)
    {
        float size = 1f / slotsPerRow;
        int x = index % slotsPerRow;
        int y = index / slotsPerRow;
        return new Rect(size * x + size * 0.1f, -size * y - size * 0.1f, size * 0.8f, size * 0.8f);
    }
}
