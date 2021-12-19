using System.Collections;
using System.Collections.Generic;
using TitanCore.Core;
using TitanCore.Data.Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemDisplay : MonoBehaviour
{

    private static Color utColor = new Color(0.7354136f, 0.1839623f, 1f, 1);

    public static Color GetTierColor(ItemTier tier)
    {
        switch (tier)
        {
            case ItemTier.Untiered:
                return utColor;
            default:
                return Color.white;
        }
    }

    public Image itemImage;

    public Image placeholder;

    public TextMeshProUGUI cornerText;

    public bool genericPlaceholder;

    //public bool overridePlaceholder;

    public Sprite placeholderOverride;

    public bool showTierLabel = true;

    private SlotType placeholderType = SlotType.Generic;

    private bool hidden = false;

    private Item item = Item.Blank;

    public void SetPlaceholderType(SlotType type)
    {
        placeholderType = type;
        placeholder.sprite = placeholderOverride != null ? placeholderOverride : TextureManager.GetPlaceholder(type);
        if (!itemImage.gameObject.activeSelf)
            placeholder.gameObject.SetActive(genericPlaceholder || placeholderType != SlotType.Generic);
    }

    public void SetItem(Item item)
    {
        this.item = item;

        UpdateActive();

        if (!item.IsBlank)
        {
            var info = item.GetInfo();

            itemImage.sprite = TextureManager.GetDisplaySprite(info);
            itemImage.material = MaterialManager.GetUIMaterial(item.enchantLevel);

            if (info is EquipmentInfo equip)
            {
                cornerText.text = equip.GetTierDisplay();
                cornerText.color = GetTierColor(equip.tier);
            }
            else if (item.count > 1)
            {
                cornerText.text = "x" + item.count;
                cornerText.color = GetTierColor(ItemTier.Starter);
            }
            else
            {
                cornerText.text = "";
                cornerText.color = GetTierColor(ItemTier.Starter);
            }
        }
    }

    private void UpdateActive()
    {
        placeholder.gameObject.SetActive(item.IsBlank && (genericPlaceholder || placeholderType != SlotType.Generic) && !hidden);
        itemImage.gameObject.SetActive(!item.IsBlank && !hidden);
        cornerText.gameObject.SetActive(!item.IsBlank && (showTierLabel || item.count > 1) && !hidden);
    }

    public void SetHidden(bool hidden)
    {
        this.hidden = hidden;
        UpdateActive();
    }
}
