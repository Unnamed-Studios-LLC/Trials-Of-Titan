using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;

public class ExportSpriteAtlas : MonoBehaviour
{
    public SpriteAtlas spriteAtlas;

    public bool exportOnAwake;

    private void Awake()
    {
        if (exportOnAwake)
            Export();
    }

    public void Export()
    {
#if UNITY_EDITOR
        var filePath = EditorUtility.SaveFilePanel("Export Sheet", null, null, "png");
        if (string.IsNullOrWhiteSpace(filePath)) return;
        var sprites = new Sprite[1];
        spriteAtlas.GetSprites(sprites);
        var texture = sprites[0].texture;
        File.WriteAllBytes(filePath, texture.EncodeToPNG());
#endif
    }
}
