using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Discord;
using System;
using TitanCore.Iap;

#if UNITY_STANDALONE

public class DiscordManager
{
    private const long Discord_Client_Id = 696590338407792691;

    private static DateTime epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public Discord.Discord discord;

    private Dispatcher dispatcher = new Dispatcher();

    private Activity currentActivity;

    public void Init()
    {
        discord = new Discord.Discord(Discord_Client_Id, (ulong)CreateFlags.Default);
        discord.SetLogHook(LogLevel.Debug, (level, message) =>
        {
            Debug.Log($"Log[{level}] {message}");
        });

        currentActivity = new Activity()
        {
            Type = ActivityType.Playing,
            ApplicationId = Discord_Client_Id,
            Name = "Project Titan",
            State = "In Menus",
            Timestamps = new ActivityTimestamps()
            {
                Start = (long)(DateTime.UtcNow - epochStart).TotalSeconds
            },
            Assets = new ActivityAssets()
            {
                LargeImage = "app-icon"
            }
        };

        if (Constants.Store_Type == TitanCore.Iap.StoreType.Discord)
        {
            var store = discord.GetStoreManager();
            store.OnEntitlementCreate += OnEntitlementCreate;
        }

        UpdateActivity();
    }

    private void OnEntitlementCreate(ref Discord.Entitlement entitlement)
    {
        VerifyEntitlement(entitlement);
    }

    public void CheckEntitlements()
    {
        var store = discord.GetStoreManager();
        store.FetchEntitlements(_ =>
        {
            if (_ != Result.Ok) return;
            ClearEntitlements();
        });
    }

    private void ClearEntitlements()
    {
        var store = discord.GetStoreManager();
        foreach (var entitlement in store.GetEntitlements())
        {
            VerifyEntitlement(entitlement);
        }
    }

    private void VerifyEntitlement(Entitlement entitlement)
    {
        if (!IapProduct.discordIdToProducts.TryGetValue(entitlement.SkuId, out var product)) return;
        WebClient.SendDiscordPurchaseVerify(entitlement.Id.ToString(), response =>
        {
            if (response.exception != null)
            {
                dispatcher.Push(() =>
                {
                    ApplicationAlert.Show("Uh oh.", "Unable to connect to server to verify purchase.", null, "Okay");
                    DiscordStoreManager.onPurchaseComplete?.Invoke();
                });
            }
            else if (!response.item.success)
            {
                dispatcher.Push(() =>
                {
                    ApplicationAlert.Show("Uh oh.", "Failed to verify purchase.", null, "Okay");
                    DiscordStoreManager.onPurchaseComplete?.Invoke();
                });
            }
            else
            {
                dispatcher.Push(() =>
                {
                    ApplicationAlert.Show("Success", "Purchase successful!", null, "Okay");
                    DiscordStoreManager.onPurchaseComplete?.Invoke();
                });
            }
        });
    }

    private void UpdateActivity()
    {
        discord.GetActivityManager().UpdateActivity(currentActivity, (result) =>
        { });
    }

    public void UpdateState(string state)
    {
        currentActivity.State = state;
        UpdateActivity();
    }

    public void Update()
    {
        discord.RunCallbacks();
        dispatcher.RunActions();
    }

    public void OnApplicationQuit()
    {
        discord.Dispose();
    }
}

#endif