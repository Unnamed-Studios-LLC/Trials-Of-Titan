using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils.NET.Geometry;

[RequireComponent(typeof(Camera))]
public class ObliqueProjection : MonoBehaviour
{
    private Camera mainCamera;

    public float angle = 180f;

    public float zScale = 1f;

    public float zOffset = -10f;

    public float orthoHeight = 10;

    private Option mapScale;

    void Awake()
    {
        mainCamera = GetComponent<Camera>();
        mapScale = Options.Get(OptionType.MapScale);
        mapScale.AddFloatCallback(OnMapScale);
        Apply();
    }

    private void OnMapScale(float value)
    {
        Apply();
    }

    public void Apply()
    {
#if UNITY_IOS || UNITY_ANDROID
        orthoHeight = 8;//Mathf.Clamp(Screen.height / 80f, 10, 14);
#else
        orthoHeight = Mathf.Clamp(Screen.height / 80f, 10, 14);
#endif
        orthoHeight *= Options.Get(OptionType.MapScale).GetFloat();

        mainCamera.orthographic = true;
        mainCamera.orthographicSize = orthoHeight;
        var orthoWidth = ((Screen.width - Screen.height * 0.25f) / (float)Screen.height) * orthoHeight;
        var m = Matrix4x4.Ortho(-orthoWidth, orthoWidth, -orthoHeight, orthoHeight, mainCamera.nearClipPlane, mainCamera.farClipPlane);
        var s = zScale / orthoHeight;
        m[0, 2] = +s * Mathf.Sin(Mathf.Deg2Rad * -angle);
        m[1, 2] = -s * Mathf.Cos(Mathf.Deg2Rad * -angle);
        m[0, 3] = -zOffset * m[0, 2];
        m[1, 3] = -zOffset * m[1, 2];
        mainCamera.projectionMatrix = m;
        mainCamera.transparencySortMode = TransparencySortMode.CustomAxis;
        mainCamera.transparencySortAxis = mainCamera.transform.up;

#if !UNITY_IOS && !UNITY_ANDROID
        mainCamera.pixelRect = new UnityEngine.Rect(0, 0, Screen.width - Screen.height * 0.25f, Screen.height);
#endif
    }

    void OnEnable()
    {
        Apply();
    }

    // Update is called once per frame
    void OnDisable()
    {
        mainCamera?.ResetProjectionMatrix();
        mapScale.RemoveFloatCallback(OnMapScale);
    }

    private void OnDestroy()
    {
        mapScale.RemoveFloatCallback(OnMapScale);
    }

    private void LateUpdate()
    {
        Resize();
    }

    private Int2 storedSize;

    public void Resize()
    {
        if (!ScreenUtils.ScreenChangedSize(ref storedSize)) return;
        Apply();
    }
}
