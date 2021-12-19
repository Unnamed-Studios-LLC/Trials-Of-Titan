using Mobile;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TitanCore.Core;
using TitanCore.Data.Items;
using TitanCore.Net.Packets.Client;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VaultMenu : MonoBehaviour
{
    private enum SortMethod
    {
        None,
        Tier,
        SlotType
    }

    private const int Pc_Slots_Per_Row_Small = 22;

    private const int Pc_Slots_Per_Row_Medium = 14;

    private const int Pc_Slots_Per_Row_Large = 8;

    private const int Mobile_Slots_Per_Row = 8;

    public GameObject slotPrefab;

    public RectTransform purchaseSlot;

    public RectTransform content;

    public TMP_Dropdown filterDropdown;

    public TMP_Dropdown sortDropdown;

    public Color searchResultColor;

    public Color defaultSlotColor;

    public RectTransform sizeTo;

    public SideMenuManager sideMenuManager;

    public SideMenu inventoryMenu;

    private VaultChest vaultChest;

    private Item[] items;

    private List<Slot> slots = new List<Slot>();

    private SlotType filter = SlotType.Generic;

    private SortMethod sort = SortMethod.None;

    private string searchPattern = "";

    private Option vaultSlotSize;

    public TextMeshProUGUI vaultSlotSizeLabel;

    private void Awake()
    {
        filterDropdown.AddOptions(((SlotType[])Enum.GetValues(typeof(SlotType))).Select(_ => _.ToString()).ToList());
        sortDropdown.AddOptions(((SortMethod[])Enum.GetValues(typeof(SortMethod))).Select(_ => _.ToString()).ToList());

        vaultSlotSize = Options.Get(OptionType.VaultSlotSize);
        UpdateVaultSlotLabel();
    }

    public void Toggle(VaultChest vaultChest)
    {
        if (gameObject.activeSelf)
            Hide();
        else
            Show(vaultChest);
    }

    public void Show(VaultChest vaultChest)
    {
        filter = SlotType.Generic;

        var rectTransform = GetComponent<RectTransform>();
#if UNITY_IOS || UNITY_ANDROID
        sideMenuManager.OpenMenu(inventoryMenu);

        sizeTo.ForceUpdateRectTransforms();
        rectTransform.offsetMax = new Vector2(-sizeTo.rect.width, 0);
#else
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
#endif

        gameObject.SetActive(true);
        SetVaultChest(vaultChest);
    }

    public void Hide()
    {
        vaultChest = null;
        items = null;
        gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        UpdateItems();
    }

    private void SetVaultChest(VaultChest vaultChest)
    {
        this.vaultChest = vaultChest;
        UpdateItems();
    }

    private void LayoutSlots()
    {
        float width = content.rect.width;
        for (int i = 0; i < vaultChest.items.Length; i++)
        {
            var slot = GetSlot(i, width);
        }

        LayoutPurchaseButton();
    }

    public void VaultSlotSizeClicked()
    {
        var value = vaultSlotSize.GetInt();
        value = (value + 1) % 3;
        vaultSlotSize.SetInt(value);

        UpdateVaultSlotLabel();

        ClearSlots();
        var chest = vaultChest;
        vaultChest = null;
        items = null;
        SetVaultChest(chest);
    }

    private void UpdateVaultSlotLabel()
    {
        if (vaultSlotSizeLabel == null) return;
        vaultSlotSizeLabel.text = ((VaultSlotSize)vaultSlotSize.GetInt()).ToString();
    }

    public void FilterChanged(int option)
    {
        var selectedFilter = (SlotType)Enum.Parse(typeof(SlotType), filterDropdown.options[option].text);
        filter = selectedFilter;
        UpdateFilter();
    }

    public void SortChanged(int option)
    {
        var selectedSort = (SortMethod)Enum.Parse(typeof(SortMethod), sortDropdown.options[option].text);
        sort = selectedSort;
        UpdateFilter();
    }

    public void SearchChanged(string search)
    {
        searchPattern = search;
        UpdateFilter();
    }

    private void UpdateFilter()
    {
        List<Slot> orderedSlots;
        if (string.IsNullOrWhiteSpace(searchPattern))
        {
            orderedSlots = FilterSlots(filter, slots);

            foreach (var slot in orderedSlots)
                slot.backgroundGraphic.color = defaultSlotColor;
        }
        else
        {
            var notMatching = slots.ToList();
            orderedSlots = new List<Slot>();
            //var matching = new List<Slot>();
            for (int i = 0; i < notMatching.Count; i++)
            {
                var slot = notMatching[i];
                if (slot.item.IsBlank) continue;
                if (DoesMatch(slot.item.GetInfo().name, searchPattern) || 
                    DoesMatch(slot.item.GetInfo().slotType.ToString(), searchPattern) || 
                    (slot.item.GetInfo() is EquipmentInfo equipInfo && DoesMatch(equipInfo.tier.ToString(), searchPattern)))
                {
                    orderedSlots.Add(slot);
                    notMatching.RemoveAt(i);
                    i--;
                }
            }

            foreach (var slot in orderedSlots)
                slot.backgroundGraphic.color = searchResultColor;

            foreach (var slot in notMatching)
                slot.backgroundGraphic.color = defaultSlotColor;

            orderedSlots = FilterSlots(filter, orderedSlots);
            orderedSlots.AddRange(FilterSlots(filter, notMatching));
        }

        for (int i = 0; i < orderedSlots.Count; i++)
        {
            float width = content.rect.width;
            var rect = GetSlotRect(i);
            var slot = orderedSlots[i];
            LeanTween.cancel(slot.gameObject);
            slot.rectTransform.LeanMove((Vector3)(rect.position * width), 0.3f);
        }
    }

    private List<Slot> FilterSlots(SlotType filter, List<Slot> slots)
    {
        var preFiltered = slots.ToList();
        var filtered = new List<Slot>();
        switch (filter)
        {
            case SlotType.Generic:
                //filtered.AddRange(preFiltered);
                filtered = SortSlots(preFiltered, sort);
                break;
            default:
                var filterResults = new List<Slot>();
                for (int i = 0; i < preFiltered.Count; i++)
                {
                    if (preFiltered[i].item.GetSlotType() == filter)
                    {
                        filterResults.Add(preFiltered[i]);
                        preFiltered.RemoveAt(i);
                        i--;
                    }
                }

                filterResults = SortSlots(filterResults, sort);
                preFiltered = SortSlots(preFiltered, sort);

                filtered.AddRange(filterResults);
                filtered.AddRange(preFiltered);
                break;
        }
        return filtered;
    }

    private List<Slot> SortSlots(List<Slot> slots, SortMethod sortMethod)
    {
        var sorted = new List<Slot>();
        switch (sortMethod)
        {
            case SortMethod.None:
                return slots;
            case SortMethod.Tier:
                sorted = slots.OrderByDescending(_ => _.item.IsBlank ? -2 : (_.item.GetInfo() is EquipmentInfo ? GetSortIntFromTier(((EquipmentInfo)_.item.GetInfo()).tier) : -1)).ToList();
                break;
            case SortMethod.SlotType:
                sorted = slots.OrderByDescending(_ => _.item.IsBlank ? -2 : (_.item.GetInfo() is EquipmentInfo ? (int)_.item.GetSlotType() : -1)).ToList();
                break;
        }
        return sorted;
    }

    private int GetSortIntFromTier(ItemTier tier)
    {
        switch (tier)
        {
            case ItemTier.Untiered:
                return 100;
            default:
                return (int)tier;
        }
    }

    private bool DoesMatch(string input, string pattern)
    {
        pattern = pattern.ToLower();
        pattern = pattern.Replace(" ", "");
        input = input.ToLower();
        input = input.Replace(" ", "");

        int lastFound = 0;

        for (int i = 0; i < pattern.Length; i++)
        {
            var searchCharacter = pattern[i];
            int index = input.IndexOf(searchCharacter, lastFound);
            if (index < 0) return false;
            lastFound = index + 1;
        }
        return true;
    }

    private void OnDisable()
    {
        ClearSlots();
    }

    private void ClearSlots()
    {
        foreach (var slot in slots)
            Destroy(slot.gameObject);
        slots.Clear();
    }

    private void UpdateItems()
    {
        if (vaultChest == null) return;
        if (items == vaultChest.items) return;
        items = vaultChest.items;
        LayoutSlots();
    }

    private void LayoutPurchaseButton()
    {
        float width = content.rect.width;
        int index = vaultChest?.items?.Length ?? 0;

        var last = GetSlotRect(index);
        purchaseSlot.anchoredPosition = last.position * width + new Vector2(0, 4);
        purchaseSlot.sizeDelta = last.size * width + new Vector2(0, 4);
        content.sizeDelta = new Vector2(content.sizeDelta.x, (-last.y + last.height) * width);
        purchaseSlot.GetComponent<FadeInMoveUp>().ResetPosition();
        purchaseSlot.gameObject.SetActive(true);
    }

    private int GetSlotsPerRow()
    {
#if UNITY_IOS || UNITY_ANDROID
        return Mobile_Slots_Per_Row;
#else
        switch ((VaultSlotSize)vaultSlotSize.GetInt())
        {
            case VaultSlotSize.Small:
                return Pc_Slots_Per_Row_Small;
            case VaultSlotSize.Medium:
                return Pc_Slots_Per_Row_Medium;
            default:
                return Pc_Slots_Per_Row_Large;
        }
#endif
    }

    private Rect GetSlotRect(int index)
    {
        var slotsPerRow = GetSlotsPerRow();
        float size = 1f / slotsPerRow;
        int x = index % slotsPerRow;
        int y = index / slotsPerRow;
        return new Rect(size * x + size * 0.1f, -size * y - size * 0.1f, size * 0.8f, size * 0.8f);
    }

    private Slot GetSlot(int index, float width)
    {
        Slot slot;
        if (slots.Count > index)
            slot = slots[index];
        else
            slot = CreateSlot(index);
        slot.SetOwner(vaultChest, index);
        slot.SetItem(vaultChest.GetItem(index));

        var rect = GetSlotRect(index);
        slot.rectTransform.anchoredPosition = rect.position * width;
        slot.rectTransform.sizeDelta = rect.size * width;
        slot.GetComponent<FadeInMoveUp>().ResetPosition();

        slot.gameObject.SetActive(true);
        return slot;
    }

    private Slot CreateSlot(int index)
    {
        var slot = Instantiate(slotPrefab).GetComponent<Slot>();
        slot.rectTransform.SetParent(content);
        slots.Add(slot);
        return slot;
    }

    public void BuyVaultSlot()
    {
        vaultChest.world.gameManager.client.SendAsync(new TnPurchaseVaultSlot(vaultChest.gameId));
    }
}
