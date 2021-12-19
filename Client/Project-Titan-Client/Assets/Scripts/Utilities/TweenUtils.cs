using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class TweenUtils
{

    public static void FadeInMoveUp(this Transform transform, float time, float delay, bool anchored, float topAlpha = 1)
    {
        Vector3 originalPosition;
        var rectTransform = transform as RectTransform;
        if (anchored && rectTransform != null)
            originalPosition = rectTransform.anchoredPosition;
        else
            originalPosition = transform.position;

        var offset = originalPosition + new Vector3(0, Screen.height * -0.05f, 0);

        var topGraphic = transform.gameObject.GetComponent<Graphic>();
        if (topGraphic != null)
            FadeIn(transform.gameObject, topGraphic, time, delay, topAlpha);
        foreach (var graphic in transform.gameObject.GetComponentsInChildren<Graphic>(false))
            if (graphic != topGraphic)
                FadeIn(transform.gameObject, graphic, time, delay, 1);

        if (anchored && rectTransform != null)
            rectTransform.anchoredPosition = offset;
        else
            transform.position = offset;

        if (anchored && rectTransform != null)
            LeanTween.sequence().append(delay).append(rectTransform.LeanMove(originalPosition, time).setEaseOutSine());
        else
            LeanTween.sequence().append(delay).append(transform.LeanMove(originalPosition, time).setEaseOutSine());
    }

    public static void FadeIn(GameObject gameObject, Graphic graphic, float time, float delay, float alpha)
    {
        if (graphic.GetComponent<NoFade>() != null) return;

        var color = graphic.color;
        color.a = alpha;

        var colorClear = color;
        colorClear.a = 0;

        graphic.color = colorClear;
        LeanTween.sequence().append(delay).append(LeanTween.value(gameObject, colorValue =>
        {
            graphic.color = colorValue;
        }, colorClear, color, time).setEaseOutSine());
    }

    public static void FadeOut(this GameObject gameObject, float time, float delay)
    {
        foreach (var graphic in gameObject.GetComponentsInChildren<Graphic>(true))
            FadeOut(gameObject, graphic, time, delay);
    }

    public static void FadeOut(GameObject gameObject, Graphic graphic, float time, float delay)
    {
        if (graphic.GetComponent<NoFade>() != null) return;

        var color = graphic.color;

        var colorClear = color;
        colorClear.a = 0;

        LeanTween.sequence().append(delay).append(LeanTween.value(gameObject, colorValue =>
        {
            graphic.color = colorValue;
        }, color, colorClear, time).setEaseOutSine());
    }

    public static void PopIn(this Transform transform, float time, float delay)
    {
        transform.localScale = Vector3.zero;
        LeanTween.sequence()
            .append(delay)
            .append(transform.LeanScale(Vector3.one, time).setEaseOutBack());
    }
}
