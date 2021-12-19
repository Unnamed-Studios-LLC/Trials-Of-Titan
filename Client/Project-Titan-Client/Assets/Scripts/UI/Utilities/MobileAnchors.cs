using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobileAnchors : MonoBehaviour
{
    public Vector2 anchorMin;

    public Vector2 anchorMax;

    public bool always;

    private void Awake()
    {
#if UNITY_IOS || UNITY_ANDROID
        DoMobile();
#elif UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
        DoPc();
#endif
    }

    private void DoPc()
    {
        Destroy(this);
    }

    private void DoMobile()
    {
        var rectTransform = GetComponent<RectTransform>();

        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;

        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }
    /*
#if UNITY_EDITOR
    private void OnEnable()
    {
        if (always)
        {
            DoMobile();
            return;
        }
#if UNITY_IOS
        DoMobile();
#endif
    }
#endif
    */
}
