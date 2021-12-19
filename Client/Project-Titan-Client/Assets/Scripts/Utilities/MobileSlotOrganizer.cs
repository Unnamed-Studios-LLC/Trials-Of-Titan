using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MobileSlotOrganizer : MonoBehaviour
{
    public RectTransform[] equips;

    public RectTransform[] inventory;

    public RectTransform main;

    public bool sizeOnEnable = true;

    private void OnEnable()
    {
        if (sizeOnEnable)
            Size();
    }

    public void Size()
    {
        var rectTransform = GetComponent<RectTransform>();
        var size = rectTransform.rect.size;
        var slotSection = size.y / inventory.Length;
        float spacing = slotSection * 0.14f;
        float slotSize = slotSection - spacing;
        slotSection += spacing / inventory.Length;

        for (int i = 0; i < inventory.Length; i++)
        {
            var slot = inventory[i];
            slot.anchoredPosition = new Vector2(0, -slotSection * i);
            slot.sizeDelta = new Vector2(slotSize, slotSize);
        }

        for (int i = 0; i < equips.Length; i++)
        {
            var slot = equips[i];
            slot.anchoredPosition = inventory[i + 2].anchoredPosition + new Vector2(slotSection, 0);//new Vector2(slotSection, -size.y / 2 + slotSize * 2 + spacing * 1.5f - (slotSize + spacing) * i);
            slot.sizeDelta = new Vector2(slotSize, slotSize);
        }

        var mainSize = main.rect.size;
        var mainSizeDelta = main.sizeDelta;
        var contentWidth = slotSize;
        if (equips != null && equips.Length > 0)
            contentWidth += slotSection;
        mainSizeDelta.x = contentWidth + mainSize.x - size.x;
        main.sizeDelta = mainSizeDelta;
    }
}
