using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Iap;
using UnityEngine;

public class StoreMenu : MonoBehaviour
{
    public GameObject loadingDots;

    private StoreManager manager;

    private StoreManagerState syncedState = StoreManagerState.None;

    public StoreEntry[] entries;

    private void Awake()
    {
        if (manager == null)
        {
            manager = CreateManager();
            if (manager == null)
            {
                SetState(StoreManagerState.LoadingError);
            }
            else
            {
                manager.Initialize();
                SetState(StoreManagerState.Loading);
            }
        }
    }

    private void SetState(StoreManagerState state)
    {
        if (syncedState == state) return;
        syncedState = state;

        switch (state)
        {
            case StoreManagerState.Loading:
                SetLoading();
                break;
            case StoreManagerState.LoadingError:
                SetLoadingError();
                break;
            case StoreManagerState.Ready:
                SetReady();
                break;
        }
    }

    private void SetLoading()
    {
        ShowLoading();
        foreach (var entry in entries)
        {
            entry.gameObject.SetActive(false);
        }
    }

    private void SetLoadingError()
    {
        HideLoading();
        ApplicationAlert.Show("Uh oh.", "Failed to load product info.", _ => Hide(), "Okay");
    }

    private void SetReady()
    {
        HideLoading();
        for (int i = 0; i < IapProduct.products.Length && i < entries.Length; i++)
        {
            var product = IapProduct.products[i];
            var entry = entries[i];

            manager.GetPrice(product, out var priceString);
            entry.Setup(product, priceString);
            entry.gameObject.SetActive(true);
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void ShowLoading()
    {
        loadingDots.SetActive(true);
    }

    public void HideLoading()
    {
        loadingDots.SetActive(false);
    }

    private void OnEnable()
    {
        if (manager == null) return;
        manager.Enable();
        SetState(manager.state);
    }

    private void OnDisable()
    {
        manager?.Disable();
    }

    private void LateUpdate()
    {
        if (manager == null) return;
        manager.LateUpdate();
        SetState(manager.state);
    }

    public void StartPurchase(StoreEntry entry)
    {
        ShowLoading();
        manager.StartPurchase(entry.product);
    }

    private StoreManager CreateManager()
    {
#if UNITY_STANDALONE
        if (Constants.Store_Type == StoreType.Discord)
            return new DiscordStoreManager(this);
        else
            return new SteamStoreManager(this);
#elif UNITY_IOS || UNITY_ANDROID
        return new MobileStoreManager(this);
#endif
    }
}
