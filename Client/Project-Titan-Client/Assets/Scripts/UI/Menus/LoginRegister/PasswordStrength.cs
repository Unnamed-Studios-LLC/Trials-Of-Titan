using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils.NET.Utils;

public class PasswordStrength : MonoBehaviour
{
    public TextMeshProUGUI strengthLabel;

    public Slider slider;

    public Image sliderFill;

    public Color veryWeakColor;

    public Color weakColor;

    public Color mediumColor;

    public Color strongColor;

    public void PasswordUpdated(string value)
    {
        SetStrength(value, StringUtils.GetPasswordStrength(value));
    }

    private void SetStrength(string value, Utils.NET.Utils.PasswordStrength strength)
    {
        slider.value = ((int)strength / 4f);
        switch (strength)
        {
            case Utils.NET.Utils.PasswordStrength.VeryWeak:
                sliderFill.color = veryWeakColor;
                break;
            case Utils.NET.Utils.PasswordStrength.Weak:
                sliderFill.color = weakColor;
                break;
            case Utils.NET.Utils.PasswordStrength.Medium:
                sliderFill.color = mediumColor;
                break;
            case Utils.NET.Utils.PasswordStrength.Strong:
                sliderFill.color = strongColor;
                break;
        }

        if (value.Length < 8)
            strengthLabel.text = "Invalid";
        else
            strengthLabel.text = StringUtils.Labelize(strength.ToString());
    }
}
