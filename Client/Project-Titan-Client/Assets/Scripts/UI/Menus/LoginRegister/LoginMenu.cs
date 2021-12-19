using System.Collections;
using System.Collections.Generic;
using TitanCore.Net;
using TitanCore.Net.Web;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils.NET.Utils;

public class LoginMenu : MonoBehaviour
{
    public GameObject registerMenu;

    public ChooseNameMenu chooseNameMenu;

    public TMP_InputField emailField;

    public TMP_InputField passwordField;

    public Toggle rememberMeToggle;

    public GameObject overlay;

    public GameObject accountMenu;

    private WebClient.Response<WebLoginResponse> loginResponse;

    private WebClient.Response<WebLoginResponse> forgotResponse;

    private WebClient.Response<WebDescribeResponse> describeResponse;

    private WebClient.Response<WebServerListResponse> serverListResponse;

    public TextMeshProUGUI errorLabel;

    private string currentAccessToken;

    private void Start()
    {
#if UNITY_STANDALONE
        GameDataLoader.discordManager.UpdateState("In Menus");
#endif

        for (int i = 0; i < MaterialManager.spriteUIs.Length; i++)
            MaterialManager.spriteUIs[i].SetFloat("_OutlineThickness", 1);

        currentAccessToken = Account.savedAccessToken;
        TryAutoLogin();

        if (AudioManager.TryGetSound("Hero's_Ballad", out var music))
        {
            var musicPlayer = AudioManager.GetBackgroundAudioPlayer();
            musicPlayer.ClearQueue();
            musicPlayer.Play(music, true);
        }
    }

    private void TryAutoLogin()
    {
        if (!string.IsNullOrWhiteSpace(currentAccessToken))
        {
            overlay.SetActive(true);
            WebClient.SendWebDescribe(currentAccessToken, OnDescribeResponse);
        }
    }

    public void OnRegister()
    {
        gameObject.SetActive(false);
        registerMenu.SetActive(true);

        ClearFields();
    }

    public void OnLogin()
    {
        var email = emailField.text;
        var password = passwordField.text;

        errorLabel.text = GetLoginError(email, password);
        if (!string.IsNullOrEmpty(errorLabel.text)) return;

        overlay.SetActive(true);
        WebClient.SendWebLogin(email, password, OnLoginResponse);
    }

    private string GetLoginError(string email, string password)
    {
        if (!StringUtils.IsValidEmail(email))
            return "Invalid email entered.";

        if (!NetConstants.IsValidPassword(password))
            return "Invalid password entered.";

        return "";
    }

    private void ClearFields()
    {
        emailField.text = "";
        passwordField.text = "";

        errorLabel.text = "";
    }

    private void OnLoginResponse(WebClient.Response<WebLoginResponse> response)
    {
        if (response.item == null || response.item.result != WebLoginResult.Success)
        {
            loginResponse = response;
            return;
        }

        currentAccessToken = response.item.accessToken;
        WebClient.SendWebDescribe(response.item.accessToken, OnDescribeResponse);
    }

    private void OnForgotResponse(WebClient.Response<WebLoginResponse> response)
    {
        forgotResponse = response;
    }

    private void OnDescribeResponse(WebClient.Response<WebDescribeResponse> response)
    {
        describeResponse = response;
    }

    public void OnRegisterComplete(string accessToken)
    {
        gameObject.SetActive(true);
        overlay.SetActive(true);
        currentAccessToken = accessToken;
        Account.savedAccessToken = currentAccessToken;
        ApplicationAlert.Show("Account Created!", $"Check your inbox for a verification email. You must verify your email before you can login.", UnverifiedButtonPressed, "Back", "I Verified");
    }

    public void Logout()
    {
        Account.loggedInAccessToken = null;
        Account.savedAccessToken = "";

        accountMenu.SetActive(false);
        gameObject.SetActive(true);
    }

    private void UnverifiedButtonPressed(int button)
    {
        if (button == 0)
        {
            overlay.SetActive(false);
        }
        else if (!string.IsNullOrWhiteSpace(currentAccessToken))
        {
            overlay.SetActive(true);
            WebClient.SendWebDescribe(currentAccessToken, OnDescribeResponse);
        }
        else
        {
            OnLogin();
        }
    }

    private void ShowRateLimit()
    {
        ApplicationAlert.Show("Oops!", $"You're doing that too much! Try again in a short time.", UnverifiedButtonPressed, "Ok");
    }

    public void ChooseNameFinished()
    {
        gameObject.SetActive(true);
        if (!string.IsNullOrWhiteSpace(currentAccessToken))
        {
            overlay.SetActive(true);
            WebClient.SendWebDescribe(currentAccessToken, OnDescribeResponse);
        }
    }

    public void ChooseNameBack()
    {
        gameObject.SetActive(true);
    }

    public void ForgotPassword()
    {
        var email = emailField.text;

        if (!StringUtils.IsValidEmail(email))
            errorLabel.text = "Invalid email entered.";

        if (!string.IsNullOrEmpty(errorLabel.text)) return;

        overlay.SetActive(true);
        WebClient.SendForgotPassword(email, OnForgotResponse);
    }

    private void LateUpdate()
    {
        if (forgotResponse != null)
        {
            overlay.SetActive(false);
            if (forgotResponse.exception != null)
            {
                Debug.LogError(forgotResponse.exception);
                ApplicationAlert.Show("Error", "Unable to get a response from the server", (button) =>
                {
                    if (button != 1) return;
                    TryAutoLogin();
                }, "Back", "Retry");
                errorLabel.text = "Unable to get a response from the server.";
            }
            else if (forgotResponse.item.result == WebLoginResult.RateLimitExceeded)
            {
                ShowRateLimit();
            }
            else if (forgotResponse.item.result == WebLoginResult.Success)
            {
                ApplicationAlert.Show("Sent", "Check your email for a recovery link.", (button) =>
                {

                }, "Okay");
            }
            else
            {
                switch (forgotResponse.item.result)
                {
                    case WebLoginResult.InvalidEmail:
                    case WebLoginResult.InvalidHash:
                        errorLabel.text = "Invalid credentials, please try again.";
                        break;
                    default:
                        errorLabel.text = StringUtils.Labelize(forgotResponse.item.result.ToString());
                        break;
                }
            }
            forgotResponse = null;
        }

        if (loginResponse != null)
        {
            overlay.SetActive(false);
            if (loginResponse.exception != null)
            {
                Debug.LogError(loginResponse.exception);
                ApplicationAlert.Show("Error", "Unable to get a response from the server", (button) =>
                {
                    if (button != 1) return;
                    TryAutoLogin();
                }, "Back", "Retry");
                errorLabel.text = "Unable to get a response from the server.";
            }
            else if (loginResponse.item.result == WebLoginResult.RateLimitExceeded)
            {
                ShowRateLimit();
            }
            else
            {
                switch (loginResponse.item.result)
                {
                    case WebLoginResult.InvalidEmail:
                    case WebLoginResult.InvalidHash:
                        errorLabel.text = "Invalid credentials, please try again.";
                        break;
                    default:
                        errorLabel.text = StringUtils.Labelize(loginResponse.item.result.ToString());
                        break;
                }
            }
            loginResponse = null;
        }

        if (describeResponse != null)
        {
            overlay.SetActive(false);
            if (describeResponse.exception != null)
            {
                Debug.LogError(describeResponse.exception);
                ApplicationAlert.Show("Uh oh..", $"Unable to get a response from the server", UnverifiedButtonPressed, "Back", "Retry");
            }
            else if (describeResponse.item.result == WebDescribeResult.UnverifiedEmail)
            {
                ApplicationAlert.Show("Unverified Account", $"Check your inbox for a verification email. You must verify your email before you can login.", UnverifiedButtonPressed, "Back", "Retry");
            }
            else if (describeResponse.item.result == WebDescribeResult.NameRequired)
            {
                chooseNameMenu.accessToken = currentAccessToken;
                gameObject.SetActive(false);
                chooseNameMenu.gameObject.SetActive(true);
            }
            else if (describeResponse.item.result == WebDescribeResult.RateLimitReached)
            {
                ShowRateLimit();
            }
            else if (describeResponse.item.result == WebDescribeResult.UpdateRequired)
            {
                ApplicationAlert.Show("Uh oh..", $"An update is required to play!\nRequired: {describeResponse.item.requiredBuild}\nCurrent: {NetConstants.Build_Version}", UnverifiedButtonPressed, "Ok");
            }
            else if (describeResponse.item.result != WebDescribeResult.Success)
            {
                switch (describeResponse.item.result)
                {
                    case WebDescribeResult.InvalidToken:
                        errorLabel.text = "Unable to verify login token, please login again.";
                        break;
                    default:
                        errorLabel.text = StringUtils.Labelize(describeResponse.item.result.ToString());
                        break;
                }
            }
            else
            {
                Account.describe = describeResponse.item;
                Account.loggedInAccessToken = currentAccessToken;
                Account.savedAccessToken = currentAccessToken;

#if UNITY_STANDALONE
                if (Constants.Store_Type == TitanCore.Iap.StoreType.Discord)
                {
                    GameDataLoader.discordManager.CheckEntitlements();
                }
#endif

                gameObject.SetActive(false);
                accountMenu.SetActive(true);
            }
            describeResponse = null;
        }
    }


}
