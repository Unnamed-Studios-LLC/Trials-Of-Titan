using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class KeepAspect : MonoBehaviour
{
    public enum Axis
    {
        Width,
        Height
    }

    public float aspect = 1;

    public Axis referenceAxis = Axis.Height;

    public bool onLateUpdate = false;

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        Size();
    }

    private void Start()
    {
        Size();
    }

    private void Size()
    {
        var rect = rectTransform.rect;
        var size = rect.size;

        float reference;
        if (referenceAxis == Axis.Width)
            reference = size.x;
        else
            reference = size.y;

        float result = reference * aspect;
        var sizeDelta = rectTransform.sizeDelta;
        if (referenceAxis == Axis.Width)
            sizeDelta.y = result;
        else
            sizeDelta.x = result;

        rectTransform.sizeDelta = sizeDelta;
    }

    private void LateUpdate()
    {
        if (!onLateUpdate) return;
        Size();
    }
}
