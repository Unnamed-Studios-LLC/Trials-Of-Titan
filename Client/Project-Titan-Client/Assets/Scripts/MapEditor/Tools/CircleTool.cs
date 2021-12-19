using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils.NET.Geometry;

public class CircleTool : MapEditorTool
{
    public override MapEditorToolType Type => MapEditorToolType.Circle;

    public int diameter;

    public bool clearMode;

    public override void DoTool(int x, int y)
    {
        for (int yy = Mathf.FloorToInt(-diameter / 2f); yy < Mathf.CeilToInt(diameter / 2f); yy++)
            for (int xx = Mathf.FloorToInt(-diameter / 2f); xx < Mathf.CeilToInt(diameter / 2f); xx++)
            {
                var offset = new Int2(xx, yy);
                if (offset.Length > diameter * 0.505f) continue;
                if (clearMode)
                    mapEditor.EraseObject(x + xx, y + yy);
                else
                    mapEditor.CreateObject(x + xx, y + yy);
            }
    }

    public override void EndTool(int x, int y)
    {

    }

    public override void StartTool(int x, int y)
    {
        DoTool(x, y);
    }
}
