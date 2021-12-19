using UnityEngine;
using System.Collections;
using UnityEngine.U2D;
using System.Collections.Generic;
using TitanCore.Data.Components.Textures;
using TitanCore.Data;
using TitanCore.Core;
using System;

public class TextureManager : MonoBehaviour
{
    /// <summary>
    /// All sprites from the ui sprite sheet
    /// </summary>
    private static Dictionary<string, Sprite> uiSprites = new Dictionary<string, Sprite>();

    /// <summary>
    /// Dictionary of all sprites keyed to their name 
    /// </summary>
    private static Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();

    /// <summary>
    /// Metadata for each game sprite
    /// </summary>
    private static Dictionary<Sprite, SpriteMetaData> metaDatas = new Dictionary<Sprite, SpriteMetaData>();

    /// <summary>
    /// Sprites for each status effect
    /// </summary>
    private static Dictionary<StatusEffect, Sprite> statusEffects = new Dictionary<StatusEffect, Sprite>();

    /// <summary>
    /// Sprites for each status effect
    /// </summary>
    private static Dictionary<EmoteType, Sprite> emoteSprites = new Dictionary<EmoteType, Sprite>();

    /// <summary>
    /// Sprite array used to copy sprites from atlases
    /// </summary>
    private static Sprite[] spriteArray;

    public static void Init(params SpriteAtlas[] spritesheets)
    {
        sprites.Clear();
        int max = 0;
        foreach (var sheet in spritesheets)
            max = Math.Max(max, sheet.spriteCount + 1);

        spriteArray = new Sprite[max];
        foreach (var atlas in spritesheets)
            AddSprites(atlas);
        spriteArray = null;

        foreach (var effect in (StatusEffect[])Enum.GetValues(typeof(StatusEffect)))
        {
            statusEffects.Add(effect, GetSprite(effect.ToString()));
        }

        foreach (var emoteType in (EmoteType[])Enum.GetValues(typeof(EmoteType)))
        {
            emoteSprites.Add(emoteType, GetSprite(emoteType.ToString() + "-emote"));
        }
    }

    /// <summary>
    /// Adds all of the sprites from a given sprite atlas into the texture management system
    /// </summary>
    /// <param name="atlas"></param>
    private static void AddSprites(SpriteAtlas atlas)
    {
        int count = atlas.GetSprites(spriteArray);
        for (int i = 0; i < count; i++)
        {
            var sprite = spriteArray[i];
            AddSprite(sprite.name.Substring(0, sprite.name.Length - 7), sprite); // trim the (Clone) part of the name
        }
    }

    private static void AddSprite(string name, Sprite sprite)
    {
        if (sprites.ContainsKey(name))
        {
            Debug.LogError($"The sprite named '{name}' already exists!");
            return;
        }
        sprites.Add(name, sprite);
        var metaData = new SpriteMetaData(sprite);
        metaDatas.Add(sprite, metaData);
    }

    public static Sprite GetSprite(string name)
    {
        if (!sprites.TryGetValue(name, out var sprite))
            return null;
        return sprite;
    }

    public static Sprite GetDisplaySprite(ushort type)
    {
        if (!GameData.objects.TryGetValue(type, out var info))
            return null;
        return GetDisplaySprite(info);
    }

    public static Sprite GetEmoteSprite(EmoteType type)
    {
        if (!emoteSprites.TryGetValue(type, out var sprite))
            return null;
        return sprite;
    }

    public static Sprite GetDisplaySprite(GameObjectInfo info)
    {
        if (info.textures.Length == 0)
            return null;
        return GetSprite(info.textures[0].displaySprite);
    }

    public static SpriteMetaData GetMetaData(Sprite sprite)
    {
        if (!metaDatas.TryGetValue(sprite, out var metaData))
            return null;
        return metaData;
    }

    public static Sprite GetUISprite(string name)
    {
        if (uiSprites.TryGetValue(name, out var sprite))
            return sprite;
        return null;
    }

    public static Sprite GetPlaceholder(SlotType slotType)
    {
        return GetUISprite("Placeholder " + slotType);
    }

    public static void SetUISprites(Sprite[] allUiSprites)
    {
        uiSprites.Clear();
        foreach (var sprite in allUiSprites)
            uiSprites.Add(sprite.name, sprite);
    }

    public static Sprite GetStatusEffect(StatusEffect effect)
    {
        return statusEffects[effect];
    }
}
