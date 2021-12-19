using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class DrawTool : MapEditorTool
{
    public override MapEditorToolType Type => MapEditorToolType.Draw;

    public override void DoTool(int x, int y)
    {
        mapEditor.CreateObject(x, y);
    }

    public override void EndTool(int x, int y)
    {

    }

    public override void StartTool(int x, int y)
    {
        DoTool(x, y);
    }
}
