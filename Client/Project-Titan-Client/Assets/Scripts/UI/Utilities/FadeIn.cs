using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeIn : MonoBehaviour
{
    public float delay;

    public float time = 0.3f;

    public float alpha = 1;

    public bool fadeChildren = false;

    private void OnEnable()
    {
        if (fadeChildren)
        {
            foreach (var graphic in transform.gameObject.GetComponentsInChildren<Graphic>(true))
                TweenUtils.FadeIn(transform.gameObject, graphic, time, delay, alpha);
        }
        else
        {
            var graphic = GetComponent<Graphic>();
            if (graphic == null) return;
            TweenUtils.FadeIn(transform.gameObject, graphic, time, delay, alpha);
        }
    }
}
