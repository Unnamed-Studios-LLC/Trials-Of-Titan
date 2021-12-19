using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Data;
using TitanCore.Data.Items;
using TitanCore.Net.Packets.Client;
using UnityEngine;
using UnityEngine.UI;

public class WardrobeMenu : GameMenu
{
    public override MenuType MenuType => MenuType.Wardrobe;

    public Image skinSelectorPrefab;

    public RectLayout rectLayout;

    public Color normalColor = Color.white;

    public Color selectedColor = Color.white;

    private Dictionary<Image, ushort> selectors = new Dictionary<Image, ushort>();

    private Image selectedSkin;

    private uint wardrobeGameId;

    public override void Setup(World world)
    {
        base.Setup(world);

        var rectTransform = (RectTransform)transform;
#if UNITY_IOS || UNITY_ANDROID
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
#else
        rectTransform.anchoredPosition = new Vector2(Screen.width - Screen.height * 0.26f, 0);
#endif

        CreateSelectors(world);

        rectLayout.gameObject.SetActive(true);
    }

    public override void Close()
    {
        base.Close();

        rectLayout.gameObject.SetActive(false);
        ClearSelectors();
    }

    public void SetGameId(uint gameId)
    {
        wardrobeGameId = gameId;
    }

    private List<GameObjectInfo> GetUnlockedSkins(GameObjectInfo currentClass)
    {
        var skins = new List<GameObjectInfo>();
        skins.Add(currentClass);
        foreach (var unlockedItem in Account.describe.unlockedItems)
        {
            var item = GameData.objects[(ushort)unlockedItem];
            if (!(item is SkinUnlockerInfo skinUnlocker)) continue;
            if (skinUnlocker.characterType != currentClass.id) continue;
            skins.Add(skinUnlocker);
        }
        return skins;
    }

    private void CreateSelectors(World world)
    {
        var unlockedSkins = GetUnlockedSkins(world.player.info);
        var selectors = new List<RectTransform>();
        var currentSkin = world.player.GetSkinInfo();
        foreach (var skin in unlockedSkins)
        {
            var selector = CreateSelector(currentSkin, skin);
            selectors.Add(selector.rectTransform);
        }

        rectLayout.rects = selectors.ToArray();
    }

    private Image CreateSelector(GameObjectInfo currentSkin, GameObjectInfo info)
    {
        var obj = Instantiate(skinSelectorPrefab.gameObject).GetComponent<Image>();
        obj.rectTransform.SetParent(rectLayout.content);

        var classPreview = obj.GetComponentInChildren<ClassPreview>();
        classPreview.SetClass(info);

        selectors.Add(obj, info.id);

        if (info.id == currentSkin.id)
        {
            selectedSkin = obj;
            obj.color = selectedColor;
        }
        else
            obj.color = normalColor;

        obj.gameObject.SetActive(true);

        return obj;
    }

    private void ClearSelectors()
    {
        foreach (var selector in selectors)
            Destroy(selector.Key.gameObject);
        selectors.Clear();
    }

    public void SelectSkin(Image selector)
    {
        if (!selectors.TryGetValue(selector, out var skinType)) return;
        world.gameManager.client.SendAsync(new TnInteract(world.clientTickId, wardrobeGameId, ((Vector2)world.player.Position).ToVec2(), skinType));
        if (selectedSkin != null)
        {
            selectedSkin.color = normalColor;
        }

        selectedSkin = selector;
        selector.color = selectedColor;
    }
}
