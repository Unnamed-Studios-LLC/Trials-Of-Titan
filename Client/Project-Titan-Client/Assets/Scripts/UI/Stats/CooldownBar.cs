using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class CooldownBar : MonoBehaviour
{
    public Image glow;

    private RectTransform rectTransform;

    public World world;

    private Color defaultColor;

    private bool full = true;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void LateUpdate()
    {
        if (world.player == null) return;

        bool isFull = world.player.rage == 100;//world.player.cooldown >= world.player.cooldownDuration;
        if (isFull && !full)
        {
            LeanTween.cancel(gameObject);
            rectTransform.localScale = Vector3.one;
            glow.color = Color.clear;

            var glowSeq = LeanTween.sequence();
            glowSeq.append(LeanTween.value(gameObject, (value) =>
            {
                glow.color = new Color(1, 1, 1, value);
            }, 0, 1, 0.2f).setEaseInSine());
            glowSeq.append(0.2f);
            glowSeq.append(LeanTween.value(gameObject, (value) =>
            {
                glow.color = new Color(1, 1, 1, value);
            }, 1, 0, 0.2f).setEaseOutSine());

            var scaleSeq = LeanTween.sequence();
            scaleSeq.append(LeanTween.scale(gameObject, new Vector3(1.3f, 1.3f, 1.3f), 0.2f).setEaseInSine());
            scaleSeq.append(LeanTween.scale(gameObject, Vector3.one, 0.2f).setEaseOutSine());
        }
        full = isFull;

        rectTransform.anchorMax = new Vector2(Mathf.Clamp01(world.player.rage / 100f), 1);
        rectTransform.offsetMax = new Vector2(0, 0);
    }
}
