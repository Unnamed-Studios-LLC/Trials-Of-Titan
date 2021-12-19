using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class OptionPanel : MonoBehaviour
{
    public OptionType type;

    protected Option option;

    private Vector3 originalPosition;

    protected virtual void Awake()
    {
        option = Options.Get(type);
    }

    protected virtual void OnEnable()
    {
    }

    protected object Default()
    {
        return Options.GetDefault(type);
    }
}
