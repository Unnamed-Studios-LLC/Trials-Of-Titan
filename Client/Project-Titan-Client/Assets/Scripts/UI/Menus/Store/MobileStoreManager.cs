using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Iap;
using TitanCore.Net.Web;
using UnityEngine;

#if UNITY_IOS || UNITY_ANDROID

public class MobileStoreManager : StoreManager, IStoreListener
{
    private IStoreController controller;

    private IExtensionProvider extensions;

    private Dispatcher dispatcher = new Dispatcher();

    public MobileStoreManager(StoreMenu menu) : base(menu)
    {
    }

    public override void Disable()
    {

    }

    public override void Enable()
    {

    }

    public override void GetPrice(IapProduct product, out string priceString)
    {
        priceString = controller.products.WithID(product.productId).metadata.localizedPriceString;
    }

    public override void Initialize()
    {
        var config = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        foreach (var product in IapProduct.products)
            config.AddProduct(product.productId, ProductType.Consumable, new IDs
            {
                { product.productId.ToLower(), GooglePlay.Name },
                { product.productId, AppleAppStore.Name }
            });

        UnityPurchasing.Initialize(this, config);
    }

    public override void LateUpdate()
    {
        dispatcher.RunActions();
    }

    public override void StartPurchase(IapProduct product)
    {
        controller.InitiatePurchase(product.productId);
        menu.ShowLoading();
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        this.controller = controller;
        this.extensions = extensions;

        SetState(StoreManagerState.Ready);
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        SetState(StoreManagerState.LoadingError);
        Debug.Log("Failed to initialize store, reason: " + error);
    }

    public void OnPurchaseFailed(Product i, PurchaseFailureReason p)
    {
        menu.HideLoading();
        ApplicationAlert.Show("Uh oh.", "Purchase failed with reason: " + p, null, "Okay");
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
    {
        var json = JObject.Parse(e.purchasedProduct.receipt);
        var store = json.Value<string>("Store");

        var product = e.purchasedProduct;
        switch (store)
        {
            case GooglePlay.Name:
                var payload = json.Value<string>("Payload");
                var payloadJson = JObject.Parse(payload);
                var purchaseJson = JObject.Parse(payloadJson.Value<string>("json"));

                WebClient.SendAndroidPurchaseVerify(purchaseJson.Value<string>("purchaseToken"), e.purchasedProduct.definition.id, response =>
                {
                    if (response.exception != null)
                    {
                        dispatcher.Push(() =>
                        {
                            ApplicationAlert.Show("Uh oh.", "Unable to connect to server to verify purchase.", null, "Okay");
                            menu.HideLoading();
                        });
                    }
                    else if (!response.item.success)
                    {
                        dispatcher.Push(() =>
                        {
                            ApplicationAlert.Show("Uh oh.", "Failed to verify purchase.", null, "Okay");
                            menu.HideLoading();
                        });
                    }
                    else
                    {
                        dispatcher.Push(() =>
                        {
                            ApplicationAlert.Show("Success", "Purchase successful!", null, "Okay");
                            menu.HideLoading();
                            controller.ConfirmPendingPurchase(product);
                        });
                    }
                });
                break;
            case AppleAppStore.Name:
                WebClient.SendiOSPurchaseVerify(json.Value<string>("Payload"), response =>
                {
                    if (response.exception != null)
                    {
                        dispatcher.Push(() =>
                        {
                            ApplicationAlert.Show("Uh oh.", "Unable to connect to server to verify purchase.", null, "Okay");
                            menu.HideLoading();
                        });
                    }
                    else if (!response.item.success)
                    {
                        dispatcher.Push(() =>
                        {
                            ApplicationAlert.Show("Uh oh.", "Failed to verify purchase.", null, "Okay");
                            menu.HideLoading();
                        });
                    }
                    else
                    {
                        dispatcher.Push(() =>
                        {
                            ApplicationAlert.Show("Success", "Purchase successful!", null, "Okay");
                            menu.HideLoading();
                            controller.ConfirmPendingPurchase(product);
                        });
                    }
                });
                break;
            default:
                return PurchaseProcessingResult.Complete;
        }

        return PurchaseProcessingResult.Pending;
    }
}

#endif