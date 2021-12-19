using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Iap;
using UnityEngine;

#if UNITY_STANDALONE

public class DiscordStoreManager : StoreManager
{
    public static Action onPurchaseComplete;
    
    private Discord.StoreManager storeManager;

    private Dispatcher dispatcher = new Dispatcher();

    public DiscordStoreManager(StoreMenu menu) : base(menu)
    {
    }

    public override void GetPrice(IapProduct product, out string priceString)
    {
        var sku = storeManager.GetSku(product.discordId);
        switch (sku.Price.Currency)
        {
            case "usd":
                priceString = $"${sku.Price.Amount / 100f}";
                break;
            case "cad":
                priceString = $"C${sku.Price.Amount / 100f}";
                break;
            case "eur":
                priceString = $"€{sku.Price.Amount / 100f}";
                break;
            case "aud":
                priceString = $"A${sku.Price.Amount / 100f}";
                break;
            case "gbp":
                priceString = $"£{sku.Price.Amount / 100f}";
                break;
            case "jpy":
                priceString = $"¥{sku.Price.Amount}";
                break;
            case "nok":
                priceString = $"{sku.Price.Amount / 100f} kr";
                break;
            case "rub":
                priceString = $"{sku.Price.Amount / 100f} rub";
                break;
            default:
                priceString = "error";
                break;
        }
    }

    public override void LateUpdate()
    {
        dispatcher.RunActions();
    }

    public override void Enable()
    {
        onPurchaseComplete = OnPurchaseComplete;
    }

    public override void Disable()
    {
        onPurchaseComplete = null;
    }

    private void OnPurchaseComplete()
    {
        menu.HideLoading();
    }

    public override void StartPurchase(IapProduct product)
    {
        menu.ShowLoading();
        storeManager.StartPurchase(product.discordId, _ =>
        {
            if (_ != Discord.Result.Ok)
            {
                Debug.Log("Discord purchase failed: " + _);
                dispatcher.Push(() =>
                {
                    menu.HideLoading();
                    ApplicationAlert.Show("Uh oh.", "Failed to start purchase", null, "Okay");
                });
            }
            else
            {
                dispatcher.Push(() =>
                {
                    menu.HideLoading();
                });
            }
        });
    }

    public override void Initialize()
    {
        storeManager = GameDataLoader.discordManager.discord.GetStoreManager();
        if (storeManager.CountSkus() != 0)
        {
            SetState(StoreManagerState.Ready);
        }
        else
        {
            storeManager.FetchSkus(_ =>
            {
                SetState(_ == Discord.Result.Ok ? StoreManagerState.Ready : StoreManagerState.LoadingError);
            });
        }
    }
}

#endif