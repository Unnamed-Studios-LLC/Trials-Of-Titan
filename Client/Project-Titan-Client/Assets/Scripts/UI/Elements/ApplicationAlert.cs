using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ApplicationAlert : MonoBehaviour
{
    private static ApplicationAlertManager manager;

    public static void SetManager(ApplicationAlertManager manager)
    {
        ApplicationAlert.manager = manager;
    }

    public static void Show(string title, string message, Action<int> buttonCallback, params string[] buttons)
    {
        var alert = manager.Create();
        if (alert == null) return;
        alert.Setup(title, message, buttonCallback, buttons);
    }

    public TextMeshProUGUI titleLabel;

    public TextMeshProUGUI messageLabel;


    public RectTransform buttonLeft;

    public TextMeshProUGUI buttonLeftLabel;


    public RectTransform buttonRight;

    public TextMeshProUGUI buttonRightLabel;

    private Action<int> buttonCallback;

    public void Setup(string title, string message, Action<int> buttonCallback, string[] buttons)
    {
        this.buttonCallback = buttonCallback;

        titleLabel.text = title;
        messageLabel.text = message;

        if (buttons.Length == 1)
        {
            buttonRight.gameObject.SetActive(false);
            buttonLeftLabel.text = buttons[0];

            buttonLeft.anchorMin = new Vector2(0.32f, buttonLeft.anchorMin.y);
            buttonLeft.anchorMax = new Vector2(0.68f, buttonLeft.anchorMax.y);

            buttonLeft.offsetMin = Vector2.zero;
            buttonLeft.offsetMax = Vector2.zero;
        }
        else
        {
            buttonLeftLabel.text = buttons[0];
            buttonRightLabel.text = buttons[1];
        }
    }

    public void OnButtonLeft()
    {
        Finish(0);
    }

    public void OnButtonRight()
    {
        Finish(1);
    }

    private void Finish(int button)
    {
        buttonCallback?.Invoke(button);
        Destroy(gameObject);
    }
}
