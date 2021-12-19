using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonListener : MonoBehaviour
{
    private const int Max_Mouse_Button = 5;

    public TMP_InputField input;

    private KeyCode[] keys;

    private Action<bool, KeyCode> callback;

    private void Awake()
    {
        keys = (KeyCode[])Enum.GetValues(typeof(KeyCode));
    }

    public void Show(Action<bool, KeyCode> callback)
    {
        this.callback = callback;
        gameObject.SetActive(true);
        EventSystem.current.SetSelectedGameObject(input.gameObject);
    }

    private void LateUpdate()
    {
        foreach (var keyCode in keys)
        {
            if (keyCode == KeyCode.Mouse0) continue;
            if (Input.GetKeyDown(keyCode))
            {
                OnKey(keyCode);
                return;
            }
        }
    }

    private void OnKey(KeyCode button)
    {
        callback?.Invoke(button != KeyCode.Escape, button);
        Hide();
    }

    private void Hide()
    {
        EventSystem.current.SetSelectedGameObject(null);
        gameObject.SetActive(false);
    }
}
