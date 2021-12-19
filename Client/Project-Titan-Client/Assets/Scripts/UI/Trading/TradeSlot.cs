using System.Collections;
using System.Collections.Generic;
using TitanCore.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TradeSlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Image blocker;

    public Image selected;

    public ItemDisplay itemDisplay;

    public TooltipManager tooltipManager;

    private Item item;

    private int index;

    private bool canSelect = false;

    private int tooltipId = -1;

    private static Color blockerColor = new Color(0.09019608f, 0.07450981f, 0.1137255f, 1);

    public bool Selected
    {
        get => selected.gameObject.activeSelf;
        set => selected?.gameObject.SetActive(value);
    }

    private void Awake()
    {
        selected?.transform.SetAsFirstSibling();
        Selected = false;
    }

    public void SetItem(Character character, Item item, int index, bool canSelect)
    {
        this.item = item;
        this.index = index;
        this.canSelect = canSelect;

        itemDisplay.SetPlaceholderType(character.GetSlotType(index));
        itemDisplay.SetItem(item);

        if (item.soulbound || index < 4)
        {
            if (blocker != null)
                blocker.color = blockerColor;
            itemDisplay.itemImage.color = new Color(1, 1, 1, 0.5f);
        }
        else
        {
            if (blocker != null)
                blocker.color = Color.clear;
            itemDisplay.itemImage.color = new Color(1, 1, 1, 1);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != 0) return;
        if (item.IsBlank || item.soulbound || !canSelect || index < 4) return;
        if (selected == null) return;

        Selected = !Selected;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ShowTooltip();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HideTooltip();
    }

    private void OnDisable()
    {
        HideTooltip();
    }

    private void OnDestroy()
    {
        HideTooltip();
    }

    private void ShowTooltip()
    {
        if (item.IsBlank) return;
        tooltipId = tooltipManager.ShowTooltip(item, canSelect);
    }

    private void HideTooltip()
    {
        if (tooltipId < 0) return;
        tooltipManager.HideTooltip(tooltipId);
        tooltipId = -1;
    }
}
