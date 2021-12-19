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

public class RegisterMenu : MonoBehaviour
{
    [Serializable]
    public class OnRegisterComplete : UnityEvent<string> { }

    public GameObject loginMenu;

    public TMP_InputField emailField;

    public TMP_InputField passwordField;

    public TMP_InputField passwordConfirmField;

    public Toggle over13Toggle;

    public Toggle termsOfServiceToggle;

    public Toggle rememberMeToggle;

    public GameObject overlay;

    public TextMeshProUGUI errorLabel;

    public OnRegisterComplete onRegisterComplete;

    private string registerResult;

    private WebClient.Response<WebRegisterResponse> registerResponse;

    private WebClient.Response<WebDescribeResponse> describeResponse;

    private string sentEmail;

    private void OnEnable()
    {
        ClearFields();
    }

    public void OnBack()
    {
        loginMenu.SetActive(true);
        gameObject.SetActive(false);
    }

    public void OnRegister()
    {
        var email = emailField.text;
        var password = passwordField.text;
        var passwordConfirm = passwordConfirmField.text;

        var error = GetFieldError(email, password, passwordConfirm);
        errorLabel.text = error;
        if (!string.IsNullOrWhiteSpace(error)) return;

        sentEmail = email;

        overlay.SetActive(true);
        WebClient.SendWebRegister(email, password, OnRegisterResponse);
    }

    private string GetFieldError(string email, string password, string passwordConfirm)
    {
        if (string.IsNullOrWhiteSpace(email)) return "Email field is blank.";
        if (!StringUtils.IsValidEmail(email)) return "Invalid email provided.";

        if (!NetConstants.IsValidPassword(password)) return "Password does not meet requirements.";
        if (!password.Equals(passwordConfirm, StringComparison.Ordinal)) return "Passwords do not match.";

        if (!over13Toggle.isOn) return "You must be at least 13 years of age to play.";

        if (!termsOfServiceToggle.isOn) return "You must agree to the terms of service to play";

        return "";
    }

    public void OnConfirmEndEdit(string value)
    {
        if (string.Equals(passwordField.text, passwordConfirmField.text, StringComparison.Ordinal)) return;
        errorLabel.text = "Passwords do not match!";
    }

    private void ClearFields()
    {
        emailField.text = "";
        passwordField.text = "";
        passwordConfirmField.text = "";
    }

    private void OnRegisterResponse(WebClient.Response<WebRegisterResponse> response)
    {
        if (response.item == null || response.item.result != WebRegisterResult.Success)
        {
            registerResponse = response;
            return;
        }
        registerResult = response.item.accessToken;
        //Account.AccessToken = response.accessToken;
        //WebClient.SendWebDescribe(response.accessToken, OnDescribeResponse);
    }

    private void OnDescribeResponse(WebClient.Response<WebDescribeResponse> response)
    {
        describeResponse = response;
    }

    private void LateUpdate()
    {
        if (registerResult != null)
        {
            overlay.SetActive(false);
            gameObject.SetActive(false);
            //ApplicationAlert.Show("Account Created!", $"Check your inbox for a verification email. You must verify your email before you can login.", null, "Ok");
            onRegisterComplete?.Invoke(registerResult);
            registerResult = null;
        }

        if (registerResponse != null)
        {
            overlay.SetActive(false);

            if (registerResponse.exception != null)
            {
                Debug.LogError(registerResponse.exception);
            }
            else
            {
                switch (registerResponse.item.result)
                {
                    case WebRegisterResult.InternalServerError:
                        errorLabel.text = "An unknown server error occured. Try again later or check your internet connection before retrying.";
                        break;
                    case WebRegisterResult.InvalidRequest:
                        errorLabel.text = "Invalid server request.";
                        break;
                    case WebRegisterResult.DuplicateName:
                        errorLabel.text = "The name given has already been taken. Try a different name.";
                        break;
                    case WebRegisterResult.DuplicateEmail:
                        errorLabel.text = "The email specified already has an account. Try forgot password if you are unable to login.";
                        break;
                }
            }
            registerResponse = null;
        }

        if (describeResponse != null)
        {
            overlay.SetActive(false);
            if (describeResponse.exception != null)
            {
                Debug.LogError(describeResponse.exception);
            }
            else if (describeResponse.item.result != WebDescribeResult.Success)
            {
                errorLabel.text = describeResponse.item.result.ToString();
            }
            else
            {
                Account.describe = describeResponse.item;
                gameObject.SetActive(false);
                //playerMenu.SetActive(true);
            }
            describeResponse = null;
        }
    }
}
