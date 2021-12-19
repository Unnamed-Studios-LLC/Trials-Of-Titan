using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class AnimateClassOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public ClassPreview[] previews;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (previews == null) return;
        foreach (var preview in previews)
            preview.animated = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (previews == null) return;
        foreach (var preview in previews)
        {
            preview.animated = false;
            preview.ResetFrame();
        }
    }
}
