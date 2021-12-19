using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class LoadingDots : MonoBehaviour
{
    public Image[] sprites;

    public float speed = 0.5f;

    public float falloff = 1;

    private float time;

    private void Awake()
    {
        DoAlpha();
    }

    private void LateUpdate()
    {
        time += Time.deltaTime * sprites.Length * speed;
        DoAlpha();
    }

    private void DoAlpha()
    {
        if (sprites == null || sprites.Length == 0) return;

        var startIndex = (int)time + sprites.Length;
        for (int i = 0; i < sprites.Length; i++)
        {
            float alpha = Mathf.Clamp01(1.0f - (((float)i / sprites.Length) / falloff));
            var index = (startIndex - i) % sprites.Length;
            var sprite = sprites[index];
            var color = sprite.color;
            color.a = alpha;
            sprite.color = color;
        }
    }
}
