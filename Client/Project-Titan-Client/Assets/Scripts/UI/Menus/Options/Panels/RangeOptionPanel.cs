using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RangeOptionPanel : OptionPanel
{
    public Slider valueSlider;

    public TMP_InputField inputField;

    public float visualDivisor = 1;

    private float value = float.MinValue;

    private float Value() => option.GetFloat();

    protected override void OnEnable()
    {
        base.OnEnable();

        UpdateValue(Value());
    }

    public void SliderChanged(float value)
    {
        option.SetFloat(value);
        UpdateValue(value);
    }

    public void TextChanged(string text)
    {
        float value = this.value;
        if (!float.TryParse(text, out value))
        {
            inputField.text = value.ToString();
            return;
        }

        value *= visualDivisor;
        value = Mathf.Clamp(value, valueSlider.minValue, valueSlider.maxValue);
        if (valueSlider.wholeNumbers)
            value = Mathf.RoundToInt(value);
        SliderChanged(value);
    }

    private void UpdateValue(float value)
    {
        if (this.value == value) return;

        valueSlider.value = value;
        inputField.text = (value / visualDivisor).ToString();
        option.SetFloat(value);
    }
}
