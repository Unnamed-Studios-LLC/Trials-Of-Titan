using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class EnumOptionPanel : OptionPanel, IPointerClickHandler
{
    public TextMeshProUGUI label;

    public string enumTypeName;

    private Array enumValues;

    private int Value() => option.GetInt();

    protected override void Awake()
    {
        base.Awake();

        enumValues = Enum.GetValues(Type.GetType(enumTypeName));
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        UpdateLabel(Value());
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        var newValue = (Value() + 1) % enumValues.Length;
        option.SetInt(newValue);
        UpdateLabel(newValue);
    }

    private void UpdateLabel(int value)
    {
        label.text = GetEnumString((Enum)enumValues.GetValue(value));
    }

    private string GetEnumString(Enum value)
    {
        var type = value.GetType();
        string name = Enum.GetName(type, value);
        if (name != null)
        {
            FieldInfo field = type.GetField(name);
            if (field != null)
            {
                DescriptionAttribute attr =
                       Attribute.GetCustomAttribute(field,
                         typeof(DescriptionAttribute)) as DescriptionAttribute;
                if (attr != null)
                {
                    return attr.Description;
                }
            }
        }
        return value.ToString();
    }
}
