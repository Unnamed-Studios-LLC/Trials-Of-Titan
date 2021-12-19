using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Core;
using TitanCore.Net;
using TitanCore.Net.Packets.Client;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AscensionMenu : GameMenu
{
    public override MenuType MenuType => MenuType.Ascension;

    public Button maxHealthButton;

    public Button speedButton;

    public Button attackButton;

    public Button defenseButton;

    public Button vigorButton;

    public TextMeshProUGUI maxHealthCost;

    public TextMeshProUGUI speedCost;

    public TextMeshProUGUI attackCost;

    public TextMeshProUGUI defenseCost;

    public TextMeshProUGUI vigorCost;

    public ItemDisplay maxHealthItemCost;

    public ItemDisplay speedItemCost;

    public ItemDisplay attackItemCost;

    public ItemDisplay defenseItemCost;

    public ItemDisplay vigorItemCost;

    private Player player;

    private uint tableGameId;

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

        OnEnable();
    }

    public void SetGameId(uint gameId)
    {
        tableGameId = gameId;
    }

    private void OnEnable()
    {
        if (world == null) return;
        player = world.player;

        UpdateLabels();
    }

    private void LateUpdate()
    {
        if (player == null) return;
        UpdateLabels();
    }

    private void UpdateLabels()
    {
        UpdateLabel(StatType.MaxHealth, maxHealthButton, maxHealthCost, maxHealthItemCost);
        UpdateLabel(StatType.Speed, speedButton, speedCost, speedItemCost);
        UpdateLabel(StatType.Attack, attackButton, attackCost, attackItemCost);
        UpdateLabel(StatType.Defense, defenseButton, defenseCost, defenseItemCost);
        UpdateLabel(StatType.Vigor, vigorButton, vigorCost, vigorItemCost);
    }

    private void UpdateLabel(StatType statType, Button button, TextMeshProUGUI cost, ItemDisplay itemCostDisplay)
    {
        var info = (TitanCore.Data.Entities.CharacterInfo)player.info;

        var level = player.GetLevel();

        if (level < NetConstants.Max_Level)
        {
            button.interactable = false;
            cost.text = $"Level {NetConstants.Max_Level} required";
            cost.color = Color.red;
            return;
        }

        var baseStat = player.GetStatBase(statType);
        var statLock = player.GetStatLock(statType);

        if (statType == StatType.MaxHealth)
        {
            if (baseStat - statLock >= NetConstants.Max_Ascension * 10)
            {
                button.interactable = false;
                cost.text = "Stat ascension maxed";
                cost.color = Color.red;
                return;
            }
        }
        else if (baseStat - statLock >= NetConstants.Max_Ascension)
        {
            button.interactable = false;
            cost.text = "Stat ascension maxed";
            cost.color = Color.red;
            return;
        }

        button.interactable = true;
        cost.color = Color.white;

        var soulCost = StatFunctions.GetAscensionCost(statType, baseStat, statLock, out var itemCost);
        cost.text = $"{Constants.Souls_Sprite} {soulCost}";
        itemCostDisplay.SetItem(itemCost);
    }

    public void Cancel()
    {
        Close();
    }

    public void AscendStat(int stat)
    {
        var statType = (StatType)stat;
        var info = (TitanCore.Data.Entities.CharacterInfo)player.info;

        var level = player.GetLevel();
        var baseStat = player.GetStatBase(statType);
        var statLock = player.GetStatLock(statType);

        if (statType == StatType.MaxHealth)
        {
            if (baseStat - statLock >= NetConstants.Max_Ascension * 10)
            {
                world.GameChat($"You have already maxed {statType} ascension!", ChatType.Error);
                return;
            }
        }
        else if (baseStat - statLock >= NetConstants.Max_Ascension)
        {
            world.GameChat($"You have already maxed {statType} ascension!", ChatType.Error);
            return;
        }

        if (level < NetConstants.Max_Level)
        {
            world.GameChat($"You must be level {NetConstants.Max_Level} to ascend your stats!", ChatType.Error);
            return;
        }

        var soulCost = StatFunctions.GetAscensionCost(statType, baseStat, statLock, out var itemCost);

        if (soulCost > player.fullSouls)
        {
            world.GameChat($"You do not have enough essence to ascend {statType}", ChatType.Error);
            return;
        }

        int itemCount = 0;
        for (int i = 4; i < 12; i++) // check item costs
        {
            var item = player.GetItem(i);
            if (item.id != itemCost.id) continue;
            itemCount += item.count;
        }

        if (itemCount < itemCost.count)
        {
            world.GameChat($"You do not have the required scrolls to ascend {statType}", ChatType.Error);
            return;
        }

        world.gameManager.client.SendAsync(new TnAscendStat(tableGameId, statType));
    }
}
