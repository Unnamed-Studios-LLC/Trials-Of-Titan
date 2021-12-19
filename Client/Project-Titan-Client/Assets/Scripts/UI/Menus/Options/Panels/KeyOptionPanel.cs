using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class KeyOptionPanel : OptionPanel, IPointerClickHandler
{
    public TextMeshProUGUI label;

    public ButtonListener listener;

    private KeyCode Value() => option.GetKey();

    protected override void OnEnable()
    {
        base.OnEnable();

        UpdateLabel(Value());
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        listener.Show(OnNewKey);
    }

    private void OnNewKey(bool didSelect, KeyCode newKey)
    {
        if (!didSelect) return;
        option.SetKey(newKey);
        UpdateLabel(newKey);
    }

    private void UpdateLabel(KeyCode value)
    {
        label.text = value.ToString();
    }
}