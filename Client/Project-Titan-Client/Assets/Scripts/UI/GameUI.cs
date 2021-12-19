using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TitanCore.Core;
using TitanCore.Net.Packets.Models;
using TitanCore.Net.Packets.Server;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class GameUI : MonoBehaviour
{
    public GameManager gameManager;

    public LoadingScreen loadingScreen;

    public Slot[] playerSlots;

    public Slot[] lootSlots;

    public Stat[] stats;

    public GameObject lootContainer;

    public BarStat healthBar;

    public SoulBar soulBar;

    public TradeMenu tradeMenu;

    public RequestPanel requestPanel;

    public InteractPanel interactPanel;

    public VaultMenu vaultMenu;

    public TextMeshProUGUI premiumCurrencyLabel;

    public TextMeshProUGUI deathCurrencyLabel;

    public TooltipManager tooltipManager;

    public Image abilityImage;

    private float lastHeight;

    protected abstract int GetOutlineThinkness(float screenHeight);

    public virtual void WorldLoading()
    {
        if (loadingScreen == null) return;
        loadingScreen.LoadingWorld();
    }

    public virtual void NewWorld(TnMapInfo mapInfo)
    {

    }

    public virtual void WorldLoaded()
    {
        if (loadingScreen != null)
        {
            loadingScreen.WorldLoaded();
        }

        var classType = (ClassType)gameManager.world.player.info.id;
        switch (classType)
        {
            case ClassType.Brewer:
                abilityImage.gameObject.SetActive(true);
                break;
            default:
                abilityImage.gameObject.SetActive(false);
                break;
        }
    }

    public virtual void OnPlayerStatsUpdated(Player player)
    {
        for (int i = 0; i < stats.Length; i++)
        {
            var stat = stats[i];
            stat.SetStat(player.GetStatBase(stat.statType), player.GetStatIncrease(stat.statType) + player.GetStatBonus(stat.statType), player);
        }
    }

    public void SetHealthValue(int health)
    {
        if (healthBar == null) return;
        healthBar.SetValue(health);
    }

    public void OnSoulsUpdated(int fullSouls, int goal)
    {
        if (soulBar == null) return;
        soulBar.SetSouls(fullSouls, goal);
    }

    public virtual void ShowContainer(IContainer container)
    {
        if (lootContainer != null)
            lootContainer.SetActive(true);
        for (int i = 0; i < lootSlots.Length; i++)
        {
            var slot = lootSlots[i];
            slot.gameObject.SetActive(true);
            slot.SetOwner(container, i);
            lootSlots[i].SetItem(container.GetItem(i));
        }
    }

    public virtual void HideContainer()
    {
        if (lootContainer != null)
            lootContainer.SetActive(false);

        for (int i = 0; i < 8 && i < lootSlots.Length; i++)
        {
            var slot = lootSlots[i];
            slot.gameObject.SetActive(false);
        }
    }

    protected virtual void LateUpdate()
    {
        if (Screen.height != lastHeight)
        {
            lastHeight = Screen.height;
            for (int i = 0; i < MaterialManager.spriteUIs.Length; i++)
                MaterialManager.spriteUIs[i].SetFloat("_OutlineThickness", GetOutlineThinkness(lastHeight));
        }

        if (gameManager.world.player != null)
        {
            var classType = (ClassType)gameManager.world.player.info.id;
            switch (classType)
            {
                case ClassType.Brewer:
                    abilityImage.sprite = TextureManager.GetSprite("BrewerPotion-" + (gameManager.world.player.abilityValue + 1));
                    break;
            }
        }
    }

    public virtual void ShowRequest(string title, IRequest request)
    {
        requestPanel.Request(title, request);
    }

    public void SetInteractable(IInteractable interactable)
    {
        if (interactPanel == null) return;
        interactPanel.SetInteractable(interactable);
    }

    public void SetPremiumCurrency(long amount)
    {
        premiumCurrencyLabel.text = $"{Constants.Premium_Currency_Sprite}{amount.ToString("N0")}";
    }

    public void SetDeathCurrency(long amount)
    {
        deathCurrencyLabel.text = $"{Constants.Death_Currency_Sprite}{amount.ToString("N0")}";
    }

    public abstract void TileDiscovered(MapTile tile);

    public abstract void PostLateUpdate();

    public abstract void PlayerDamageTaken();
}
