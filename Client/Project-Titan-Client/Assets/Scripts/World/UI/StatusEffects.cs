using System;
using System.Collections;
using System.Collections.Generic;
using TitanCore.Core;
using UnityEngine;

public class StatusEffects : MonoBehaviour
{
    private const float Sprite_Spacing = 0.62f;

    public Entity toFollow;

    private static StatusEffect[] effectTypes = (StatusEffect[])Enum.GetValues(typeof(StatusEffect));

    private Dictionary<StatusEffect, SpriteRenderer> effects = new Dictionary<StatusEffect, SpriteRenderer>();

    private void LateUpdate()
    {
        if (toFollow == null) return;

        var height = toFollow.GetHeight();
        var position = toFollow.transform.position;
        position.z -= height + 0.06f;
        transform.position = position;

        var parentScale = transform.parent.localScale.x;
        float scale;
        if (parentScale != 0)
            scale = 0.7f / parentScale;
        else
            scale = 0;
        transform.localScale = new Vector3(scale, scale, scale);

        UpdateEffects();
    }

    private void UpdateEffects()
    {
        bool changes = false;
        for (int i = 0; i < effectTypes.Length; i++)
        {
            var effect = effectTypes[i];
            if (toFollow.HasStatusEffect(effect))
            {
                if (effects.ContainsKey(effect)) continue;
                var effectSprite = toFollow.world.gameManager.objectManager.GetStatusEffectSprite(this, effect); // add effect sprite
                effects.Add(effect, effectSprite);
                changes = true;
            }
            else
            {
                if (!effects.TryGetValue(effect, out var effectSprite)) continue;
                effects.Remove(effect);
                toFollow.world.gameManager.objectManager.ReturnStatusEffectSprite(effectSprite); // remove effect sprite
                changes = true;
            }
        }

        if (!changes || effects.Count == 0) return;
        // realign sprites

        int index = 0;
        float spread = (effects.Count - 1) * Sprite_Spacing;
        foreach (var sprite in effects.Values)
        {
            sprite.transform.localPosition = new Vector3(-spread / 2 + Sprite_Spacing * index, 0, 0);
            index++;
        }
    }
}
