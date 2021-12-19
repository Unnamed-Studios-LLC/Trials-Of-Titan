using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Core;
using TitanCore.Data;
using TitanCore.Data.Components;
using TitanCore.Net.Packets.Client;
using TitanCore.Net.Packets.Models;
using TMPro;
using UnityEngine;

public class MarketDisplay : SpriteWorldObject, IInteractable
{
    public override GameObjectType ObjectType => GameObjectType.MarketDisplay;

    public string[] InteractionOptions => GetInteractOptions();

    public string InteractionTitle => purchasableItem.GetInfo().name;

    public TextMeshPro countLabel;

    public long deathCurrenyCost;

    public long premiumCurrenyCost;

    public Item purchasableItem;

    private int tooltip = -1;

    private string[] GetInteractOptions()
    {
        if (premiumCurrenyCost == 0)
        {
            return new string[] { $"{Constants.GetCurrencySprite(Constants.Death_Currency_Sprite, 0)}{deathCurrenyCost}" };
        }
        else if (deathCurrenyCost == 0)
        {
            return new string[] { $"{Constants.GetCurrencySprite(Constants.Premium_Currency_Sprite, 0)}{premiumCurrenyCost}" };
        }
        else
        {
            return new string[] { $"{Constants.GetCurrencySprite(Constants.Premium_Currency_Sprite, 0)}{premiumCurrenyCost}", $"{Constants.GetCurrencySprite(Constants.Death_Currency_Sprite, 0)}{deathCurrenyCost}" };
        }
    }

    public override void LoadObjectInfo(GameObjectInfo info)
    {
        base.LoadObjectInfo(info);

        SetSprite(null);
    }

    public override void Enable()
    {
        base.Enable();

        deathCurrenyCost = 0;
        premiumCurrenyCost = 0;
        purchasableItem = Item.Blank;
        tooltip = -1;

        transform.localEulerAngles = new Vector3(0, 0, 0);
    }

    protected override float GetRelativeScale()
    {
        return 0.8f;
    }

    protected override void ProcessStat(NetStat stat, bool first)
    {
        base.ProcessStat(stat, first);

        switch (stat.type)
        {
            case ObjectStatType.Inventory0:
                SetItem((Item)stat.value);
                break;
            case ObjectStatType.PremiumCurrency:
                premiumCurrenyCost = (long)stat.value;
                break;
            case ObjectStatType.DeathCurrency:
                deathCurrenyCost = (long)stat.value;
                break;
        }
    }

    private void SetItem(Item item)
    {
        purchasableItem = item;
        if (item.count <= 1)
            countLabel.gameObject.SetActive(false);
        else
        {
            countLabel.gameObject.SetActive(true);
            countLabel.text = item.count.ToString();
        }

        var info = purchasableItem.GetInfo();
        SetSprite(TextureManager.GetDisplaySprite(info));

        if (info.textures.Length != 0)
        {
            var texture = info.textures[0];
            SetEffect(texture.effect, texture.effectPosition);
        }
        else
            ClearEffect();
    }

    public void Interact(int option)
    {
        ObjectStatType currencyType;

        if (premiumCurrenyCost == 0)
        {
            currencyType = ObjectStatType.DeathCurrency;
        }
        else
        {
            currencyType = option == 0 ? ObjectStatType.PremiumCurrency : ObjectStatType.DeathCurrency;
        }

        world.gameManager.client.SendAsync(new TnInteract(world.clientTickId, gameId, ((Vector2)world.player.Position).ToVec2(), (int)currencyType));
    }

    public void OnEnter()
    {
        tooltip = world.gameManager.ui.tooltipManager.ShowTooltip(this);
    }

    public void OnExit()
    {
        if (tooltip == -1) return;
        world.gameManager.ui.tooltipManager.HideTooltip(tooltip);
        tooltip = -1;
    }
}
