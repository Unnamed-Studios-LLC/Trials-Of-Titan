using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Data;
using TitanCore.Iap;
using UnityEngine;

public static class Constants
{
    public static GameObjectInfo[] classes = GameData.objects.Values.Where(_ => _ is TitanCore.Data.Entities.CharacterInfo charInfo && !charInfo.notPlayable).ToArray();

    public static int maxQuests => classes.Length * 4;

    public const string Premium_Currency_Sprite = "<sprite name=\"Premium\">";

    public const string Death_Currency_Sprite = "<sprite name=\"Currency\">";

    public const string Souls_Sprite = "<sprite name=\"Souls\">";

    public const StoreType Store_Type =
#if UNITY_IOS
        StoreType.Apple;
#elif UNITY_ANDROID
        StoreType.Android;
#elif UNITY_STANDALONE
        //StoreType.Discord;
        StoreType.Steam;
#endif

    public const string Game_Scene =
#if UNITY_IOS || UNITY_ANDROID
        "MobileGameScene";
#else
        "PcGameScene";
#endif


    public static string GetCurrencySprite(string sprite, float fontSize)
    {
        return sprite;
        /*
        if (fontSize >= 18)
            return sprite;
        else
            return $"<size=18>{sprite}</size>";
        */
    }

    public static string GetClassQuestString(int classQuests)
    {
        int questStep = Mathf.CeilToInt(maxQuests / 6.0f);
        int index = Mathf.Min(classQuests / questStep, 5);
        if (classQuests >= maxQuests)
            index = 6;

        return $"<sprite name=\"ClassQuest{index}\">";
    }
}
