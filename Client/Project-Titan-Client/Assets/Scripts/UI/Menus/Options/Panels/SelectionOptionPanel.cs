using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class SelectionOptionPanel : OptionPanel
{
    public string value;

    public Image background;

    public Color selectedColor;

    public Color defaultColor;

    private string Value() => option.GetString();

    protected override void Awake()
    {
        base.Awake();

        option.AddStringCallback(UpdateColor);
    }

    private void OnDestroy()
    {
        option.RemoveStringCallback(UpdateColor);
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        UpdateColor(option.GetString());
    }

    public void OnClick()
    {
        option.SetString(value);
        UpdateColor(value);
    }

    private void UpdateColor(string value)
    {
        Color color;
        if (this.value.Equals(value, StringComparison.Ordinal))
        {
            color = selectedColor;
        }
        else
        {
            color = defaultColor;
        }
        color.a = background.color.a;
        background.color = color;
    }
}