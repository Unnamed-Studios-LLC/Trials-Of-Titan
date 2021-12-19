using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Graphic))]
public class Flash : MonoBehaviour
{
    public Color flash;

    private Graphic graphic;

    private Color graphicColor;

    private void Awake()
    {
        graphic = GetComponent<Graphic>();
        graphicColor = graphic.color;
    }

    private void LateUpdate()
    {
        graphic.color = Color.Lerp(graphicColor, flash, Mathf.Sin(Time.time * Mathf.PI) / 2f + 0.5f);
    }

    public void SetEnabled(bool enabled)
    {
        this.enabled = enabled;
        if (!enabled)
        {
            graphic.color = graphicColor;
        }
    }
}
