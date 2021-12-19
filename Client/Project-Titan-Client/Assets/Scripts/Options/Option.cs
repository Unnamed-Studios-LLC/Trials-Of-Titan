using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils.NET.Utils;

public class Option
{
    public OptionType type;

    private string key;

    private event Action<float> onFloatUpdated;

    private event Action<bool> onBoolUpdated;

    private event Action<int> onIntUpdated;

    private event Action<int[]> onIntArrayUpdated;

    private event Action<string> onStringUpdated;

    private event Action<KeyCode> onKeyUpdated;

    public Option(OptionType type)
    {
        this.type = type;
        if (Account.describe != null)
            key = Account.describe.accountId + "-" + type.ToString();
        else
            key = type.ToString();
    }

    public void AddFloatCallback(Action<float> callback)
    {
        onFloatUpdated += callback;
    }

    public void AddBoolCallback(Action<bool> callback)
    {
        onBoolUpdated += callback;
    }

    public void AddIntCallback(Action<int> callback)
    {
        onIntUpdated += callback;
    }

    public void AddIntArrayCallback(Action<int[]> callback)
    {
        onIntArrayUpdated += callback;
    }

    public void AddStringCallback(Action<string> callback)
    {
        onStringUpdated += callback;
    }

    public void AddKeyCallback(Action<KeyCode> callback)
    {
        onKeyUpdated += callback;
    }

    public void RemoveFloatCallback(Action<float> callback)
    {
        onFloatUpdated -= callback;
    }

    public void RemoveBoolCallback(Action<bool> callback)
    {
        onBoolUpdated -= callback;
    }

    public void RemoveIntCallback(Action<int> callback)
    {
        onIntUpdated -= callback;
    }

    public void RemoveIntArrayCallback(Action<int[]> callback)
    {
        onIntArrayUpdated -= callback;
    }

    public void RemoveStringCallback(Action<string> callback)
    {
        onStringUpdated -= callback;
    }

    public void RemoveKeyCallback(Action<KeyCode> callback)
    {
        onKeyUpdated -= callback;
    }

    public void SetFloat(float value)
    {
        PlayerPrefs.SetFloat(key, value);
        onFloatUpdated?.Invoke(value);
    }

    public void SetBool(bool value)
    {
        PlayerPrefs.SetInt(key, value ? 1 : 0);
        onBoolUpdated?.Invoke(value);
    }

    public void SetInt(int value)
    {
        PlayerPrefs.SetInt(key, value);
        onIntUpdated?.Invoke(value);
    }

    public void SetIntArray(int[] value)
    {
        SetString(StringUtils.ComponentsToString(',', value));
        onIntArrayUpdated?.Invoke(value);
    }

    public void SetString(string value)
    {
        PlayerPrefs.SetString(key, value);
        onStringUpdated?.Invoke(value);
    }

    public void SetKey(KeyCode keyCode)
    {
        PlayerPrefs.SetInt(key, (int)keyCode);
        onKeyUpdated?.Invoke(keyCode);
    }

    public float GetFloat()
    {
        return PlayerPrefs.GetFloat(key, (float)Options.GetDefault(type));
    }

    public bool GetBool()
    {
        return PlayerPrefs.GetInt(key, (bool)Options.GetDefault(type) ? 1 : 0) == 1;
    }

    public int GetInt()
    {
        return PlayerPrefs.GetInt(key, (int)Options.GetDefault(type));
    }

    public int[] GetIntArray()
    {
        return StringUtils.ComponentsFromString(GetString(), ',', _ => int.Parse(_)).ToArray();
    }

    public string GetString()
    {
        return PlayerPrefs.GetString(key, (string)Options.GetDefault(type));
    }

    public KeyCode GetKey()
    {
        return (KeyCode)PlayerPrefs.GetInt(key, (int)(KeyCode)Options.GetDefault(type));
    }
}
