using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Core;
using TitanCore.Data;
using TitanCore.Net.Web;
using UnityEngine;

public static class Account
{
    public static string savedAccessToken
    {
        get => PlayerPrefs.GetString("accessToken", "");
        set => PlayerPrefs.SetString("accessToken", value);
    }

    public static string loggedInAccessToken;

    public static WebDescribeResponse describe;

    public static ClassQuest GetClassQuest(ushort type)
    {
        if (describe == null) return new ClassQuest(type, 0);
        foreach (var quest in describe.classQuests)
            if (quest.classId == type)
                return quest;
        return new ClassQuest(type, 0);
    }

    public static bool HasUnlockedEmote(EmoteType emoteType)
    {
        var emoteInfo = GameData.GetEmoteInfo(emoteType);
        return HasUnlockedItem(emoteInfo.id);
    }

    public static void UnlockEmote(EmoteType emoteType)
    {
        var emoteInfo = GameData.GetEmoteInfo(emoteType);
        UnlockItem(emoteInfo.id);
    }

    public static bool HasUnlockedItem(uint itemType)
    {
        foreach (var unlocked in describe.unlockedItems)
            if (unlocked == itemType)
                return true;
        return false;
    }

    public static void UnlockItem(uint itemType)
    {
        var list = describe.unlockedItems.ToList();
        list.Add(itemType);
        describe.unlockedItems = list.ToArray();
    }

    public static WebServerInfo GetSelectedServer()
    {
        var selected = PlayerPrefs.GetString("selectedServer", null);
        if (selected == null || describe == null || describe.servers == null) return null;
        foreach (var server in describe.servers)
            if (server.name.Equals(selected))
                return server;
        return null;
    }

    public static void SetSelectedServer(WebServerInfo info)
    {
        if (info == null)
            PlayerPrefs.DeleteKey("selectedServer");
        else
            PlayerPrefs.SetString("selectedServer", info.name);

        PlayerPrefs.Save();
    }
}
