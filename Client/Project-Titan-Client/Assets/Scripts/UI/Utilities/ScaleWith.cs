using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ScaleWith : MonoBehaviour
{
    public enum ScaleAxis
    {
        Width,
        Height
    }

    public RectTransform reference;

    public ScaleAxis referenceAxis;

    public ScaleAxis targetAxis;

    public float targetScale;

    public float aspect;

    private bool scaled = false;

    // Start is called before the first frame update
    void Start()
    {
        var parent = reference.gameObject.GetComponent<ScaleWith>();
        if (parent != null)
            parent.DoFirstScale();
        DoFirstScale();
    }

#if UNITY_EDITOR
    // Update is called once per frame
    void Update()
    {
        DoScale();
    }
#endif

    public void DoFirstScale()
    {
        if (scaled) return;
        scaled = true;
        DoScale();
    }

    private void DoScale()
    {
        float size = GetAxis(reference, referenceAxis) * targetScale;
        SetSize((RectTransform)transform, targetAxis, size);
    }

    private float GetAxis(RectTransform rect, ScaleAxis axis)
    {
        switch (axis)
        {
            case ScaleAxis.Width:
                return rect.sizeDelta.x;
            case ScaleAxis.Height:
                return rect.sizeDelta.y;
        }
        return 0;
    }

    private void SetSize(RectTransform rect, ScaleAxis axis, float value)
    {
        Vector2 size;
        switch (axis)
        {
            case ScaleAxis.Width:
                size = new Vector2(value, value * aspect);
                break;
            case ScaleAxis.Height:
                size = new Vector2(value * aspect, value);
                break;
            default:
                size = new Vector2();
                break;
        }
        rect.sizeDelta = size;
    }
}
