using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Net.Packets.Client;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ChatInput : MonoBehaviour
{
    private TMP_InputField input;

    private bool editEnded = false;

    public GameObject chatInstructionLabel;

    public SideChat sideChat;

    public World world;

    public RectTransform rectTransform;

#if (UNITY_IOS || UNITY_ANDROID)

    private TouchScreenKeyboard touchKeyboard;

#endif


    private void Awake()
    {
        input = GetComponent<TMP_InputField>();
    }

    public void OnEndEdit()
    {
        editEnded = true;
    }

    private void SetChatInstructionActive(bool active)
    {
        if (chatInstructionLabel == null) return;
        chatInstructionLabel.SetActive(active);
    }

    public void StartChat()
    {
#if (UNITY_IOS || UNITY_ANDROID)
        touchKeyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default, true, false, false, false, "");
        gameObject.SetActive(true);
#else
        gameObject.SetActive(true);
        SetChatInstructionActive(false);
        EventSystem.current.SetSelectedGameObject(input.gameObject);
#endif
    }

#if UNITY_IOS || UNITY_ANDROID
    private void FitToMobileKeyboard()
    {
        if (rectTransform == null) return;
        var area = TouchScreenKeyboard.area;
        rectTransform.anchoredPosition = new Vector2(0, area.height);
    }
#endif

    private void LateUpdate()
    {
#if (UNITY_IOS || UNITY_ANDROID)
        if (touchKeyboard != null)
        {
            //Debug.Log(touchKeyboard.status);
            //Debug.Log(touchKeyboard.text);
            switch (touchKeyboard.status)
            {
                case TouchScreenKeyboard.Status.Done:
                    SendChat(touchKeyboard.text.Trim());
                    touchKeyboard = null;
                    gameObject.SetActive(false);
                    break;
                case TouchScreenKeyboard.Status.Canceled:
                    touchKeyboard = null;
                    gameObject.SetActive(false);
                    break;
            }
        }
#else
        if (!editEnded) return;
        if (Input.GetKeyDown(KeyCode.Return))
        {
            EventSystem.current.SetSelectedGameObject(null);
            gameObject.SetActive(false);
            SetChatInstructionActive(true);

            var text = input.text;
            input.text = "";

            text = text.Trim();
            SendChat(text);
        }
        editEnded = false;
#endif
    }

    private void SendChat(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

        if (!IsCommand(text))
        {
            //sideChat.AddChat(world.player.playerName, text, ChatType.Player, world.player.classQuests, world.player.rank);
            //world.player?.ShowChatBubble(text);
        }
        world.gameManager.client.SendAsync(new TnChat(text));
    }

    private bool IsCommand(string text)
    {
        return text.StartsWith("/", StringComparison.Ordinal);
    }
}