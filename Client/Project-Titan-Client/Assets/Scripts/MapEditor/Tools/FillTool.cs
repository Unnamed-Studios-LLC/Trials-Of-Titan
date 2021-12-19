using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class FillTool : MapEditorTool
{
    public override MapEditorToolType Type => MapEditorToolType.Fill;

    public bool clearMode = false;

    public bool customSample = false;

    public MapEditorObjectType sampleType;

    public override void DoTool(int x, int y)
    {

    }

    public override void EndTool(int x, int y)
    {

    }

    public override void StartTool(int x, int y)
    {
        mapEditor.Fill(x, y, customSample ? sampleType : mapEditor.objLayout.selectedType, clearMode);
    }

    public void IncrementSampleType()
    {
        if (customSample)
        {
            var i = (int)sampleType;
            i++;
            if (i <= 2)
                sampleType = (MapEditorObjectType)i;
            else
            {
                customSample = false;
            }
        }
        else
        {
            customSample = true;
            sampleType = MapEditorObjectType.Object;
        }
    }
}