using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public enum MapEditorToolType
{
    Draw,
    Erase,
    Fill,
    Circle
}

public abstract class MapEditorTool : MonoBehaviour
{
    public abstract MapEditorToolType Type { get; }

    private static Color selectedColor = new Color(0.6179246f, 0.9929499f, 1);

    private static Color normalColor = Color.white;

    public Image image;

    public MapEditor mapEditor;

    public KeyCode selectKey;

    public void SetSelected()
    {
        image.color = selectedColor;
    }

    public void SetNormal()
    {
        image.color = normalColor;
    }

    public abstract void StartTool(int x, int y);

    public abstract void DoTool(int x, int y);

    public abstract void EndTool(int x, int y);
}
