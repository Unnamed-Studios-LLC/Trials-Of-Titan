#if UNITY_STANDALONE

using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Iap;

public class SteamStoreManager : StoreManager
{
    private Dispatcher dispatcher = new Dispatcher();

    public SteamStoreManager(StoreMenu menu) : base(menu)
    {
        SteamUser.OnMicroTxnAuthorizationResponse += OnPurchaseFinished;
    }

    public override void Enable()
    {

    }

    public override void Disable()
    {

    }

    public override void GetPrice(IapProduct product, out string priceString)
    {
        priceString = product.priceString;
    }

    public override void Initialize()
    {
        SetState(StoreManagerState.Ready);
    }

    public override void LateUpdate()
    {
        dispatcher.RunActions();
    }

    public override void StartPurchase(IapProduct product)
    {
        if (!SteamClient.IsValid)
        {
            ApplicationAlert.Show("Uh oh.", "Failed to start purchase. Steam is not available", null, "Okay");
            menu.HideLoading();
            return;
        }

        if (!SteamClient.IsLoggedOn)
        {
            ApplicationAlert.Show("Uh oh.", "Failed to start purchase. User not logged into steam.", null, "Okay");
            menu.HideLoading();
            return;
        }

        menu.ShowLoading();
        WebClient.SendSteamPurchaseStart(SteamClient.SteamId.Value.ToString(), SteamApps.GameLanguage, product.productId, response =>
        {
            if (response.exception != null)
            {
                dispatcher.Push(() =>
                {
                    ApplicationAlert.Show("Uh oh.", "Unable to connect to server to start purchase.", null, "Okay");
                    menu.HideLoading();
                });
            }
            else if (!response.item.success)
            {
                dispatcher.Push(() =>
                {
                    ApplicationAlert.Show("Uh oh.", "Server failed to start purchase.", null, "Okay");
                    menu.HideLoading();
                });
            }
        });
    }

    private ulong sentOrderId = 0;

    private void OnPurchaseFinished(AppId appId, ulong orderId, bool success)
    {
        if (!success)
        {
            ApplicationAlert.Show("Uh oh.", "Failed to complete purchase.", null, "Okay");
            menu.HideLoading();
            return;
        }

        if (sentOrderId == orderId) return;
        sentOrderId = orderId;

        WebClient.SendSteamPurchaseVerify(orderId.ToString(), response =>
        {
            if (response.exception != null)
            {
                dispatcher.Push(() =>
                {
                    ApplicationAlert.Show("Uh oh.", "Unable to connect to server to finalize purchase.", null, "Okay");
                    menu.HideLoading();
                    sentOrderId = 0;
                });
            }
            else if (!response.item.success)
            {
                dispatcher.Push(() =>
                {
                    ApplicationAlert.Show("Uh oh.", "Failed to verify purchase.", null, "Okay");
                    menu.HideLoading();
                    sentOrderId = 0;
                });
            }
            else
            {
                dispatcher.Push(() =>
                {
                    ApplicationAlert.Show("Success", "Purchase successful!", null, "Okay");
                    menu.HideLoading();
                    sentOrderId = 0;
                });
            }
        });
    }
}
#endif