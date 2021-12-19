using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine.EventSystems;

public class BoolOptionPanel : OptionPanel, IPointerClickHandler
{
    public TextMeshProUGUI label;

    public string onText = "On";

    public string offText = "Off";

    private bool DefaultBool() => (bool)Default();

    private bool Value() => option.GetBool();

    protected override void OnEnable()
    {
        base.OnEnable();

        UpdateLabel(Value());
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        var newValue = !Value();
        option.SetBool(newValue);
        UpdateLabel(newValue);
    }

    private void UpdateLabel(bool value)
    {
        label.text = value ? onText : offText;
    }
}
