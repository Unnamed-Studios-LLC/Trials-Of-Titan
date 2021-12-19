using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TitanButton : Button
{
    public RectTransform content;

    public TextMeshProUGUI textLabel;

    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        base.DoStateTransition(state, instant);

        UpdateTextLabelPosition(state);
    }

    private void UpdateTextLabelPosition(SelectionState state)
    {
        switch (state)
        {
            case SelectionState.Normal:
            case SelectionState.Selected:
            case SelectionState.Disabled:
                SetLabelYOffset(0);
                break;
            case SelectionState.Highlighted:
                SetLabelYOffset(4);
                break;
            case SelectionState.Pressed:
                SetLabelYOffset(-4);
                break;
        }
    }

    private void SetLabelYOffset(float offset)
    {
        if (content == null) return;
        content.offsetMin = new Vector2(4, 8 + offset);// * content.lossyScale;
        content.offsetMax = new Vector2(-4, -8 + offset);// * content.lossyScale;

        var rectTransform = (RectTransform)transform;
        
        //textLabel.rectTransform.offsetMin = new Vector2(0, offset);
        //textLabel.rectTransform.offsetMax = new Vector2(0, offset);
    }
}
