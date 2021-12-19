using System;
using System.Collections;
using System.Collections.Generic;
using TitanCore.Net;
using TitanCore.Net.Web;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils.NET.Utils;

public class ChooseNameMenu : MonoBehaviour
{
    [Serializable]
    public class OnBack : UnityEvent { }

    [Serializable]
    public class OnComplete : UnityEvent { }

    public TMP_InputField nameField;

    public TMP_InputField reservationField;

    public Toggle reservationToggle;

    public TextMeshProUGUI errorLabel;

    public OnBack onBack;

    public OnComplete onComplete;

    public GameObject overlay;

    public string accessToken;

    private WebClient.Response<WebNameChangeResponse> response;

    private void OnEnable()
    {
        ClearFields();
        response = null;
    }

    private void ClearFields()
    {
        nameField.text = "";
        reservationField.text = "";
        reservationToggle.isOn = false;
    }

    public void ReservationToggleChanged(bool toggled)
    {
        reservationField.interactable = toggled;
    }

    public void Back()
    {
        gameObject.SetActive(false);
        onBack?.Invoke();
    }

    public void Choose()
    {
        overlay.SetActive(true);

        string token = accessToken;
        string reservation = reservationToggle.isOn ? reservationField.text : null;
        string toName = nameField.text;
        string fromName = "*";

        errorLabel.text = GetInputError(toName);
        if (!string.IsNullOrEmpty(errorLabel.text)) return;

        WebClient.SendChangeName(token, fromName, toName, reservation, (response) =>
        {
            this.response = response;
        });
    }

    private string GetInputError(string name)
    {
        if (!NetConstants.IsValidUsername(name))
            return "Invalid username";

        return "";
    }

    private void LateUpdate()
    {
        if (response != null)
        {
            overlay.SetActive(false);
            if (response.exception != null)
            {
                Debug.LogError(response.exception);
            }
            else if (response.item.result == WebNameChangeResult.Success)
            {
                gameObject.SetActive(false);
                onComplete?.Invoke();
            }
            else
            {
                errorLabel.text = StringUtils.Labelize(response.item.result.ToString());
            }

            response = null;
        }
    }
}
