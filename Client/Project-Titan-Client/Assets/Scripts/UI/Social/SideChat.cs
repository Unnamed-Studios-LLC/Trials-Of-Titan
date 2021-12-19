using System.Collections;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using TMPro;
using UnityEngine;

public enum ChatType
{
    Player,
    Enemy,
    PrivateMessage,
    Error,
    ServerInfo,
    ClientInfo
}

public class SideChat : MonoBehaviour
{
    private class ChatText
    {
        public TextMeshProUGUI label;

        public float time;
    }

    private static Color clientInfoColor = new Color(0, 0.7f, 0.93f, 1);

    public GameObject sideChatPrefab;

    public RectTransform rectTransform;

    private List<ChatText> sideChats = new List<ChatText>();

    public void AddChat(string text, ChatType type) => AddChat("", text, type);

    public void AddChat(string owner, string text, ChatType type, byte classQuests = 0, Rank rank = Rank.Player)
    {
        var builder = new StringBuilder();

        switch (type)
        {
            case ChatType.Player:
                builder.Append("<color=#00ff00>[  ");
                if (rank == Rank.Admin)
                    builder.Append($"<sprite name=\"AdminCrown\"> ");
                builder.Append(Constants.GetClassQuestString(classQuests));
                builder.Append(owner);
                builder.Append("] </color>");
                break;
            case ChatType.Enemy:
                builder.Append("<color=#FF9900>[");
                builder.Append(owner);
                builder.Append("] </color>");
                break;
            case ChatType.PrivateMessage:
                builder.Append("[");
                builder.Append(owner);
                builder.Append("] ");
                break;
        }

        builder.Append(ChatBox.CleanText(text));

        var parentRect = rectTransform.rect;

        var label = Instantiate(sideChatPrefab).GetComponent<TextMeshProUGUI>();
        label.text = builder.ToString();
        label.rectTransform.SetParent(transform);
        label.fontSize = parentRect.height * 0.4f;
        label.color = ColorForType(type);
        label.rectTransform.sizeDelta = new Vector2(parentRect.width * 0.4f, label.fontSize);
        label.ForceMeshUpdate();

        sideChats.Insert(0, new ChatText
        {
            label = label,
            time = 0
        });

        PositionChats();
    }

    private Color ColorForType(ChatType type)
    {
        switch (type)
        {
            case ChatType.ServerInfo:
                return Color.yellow;
            case ChatType.ClientInfo:
                return clientInfoColor;
            case ChatType.Error:
                return Color.red;
            case ChatType.PrivateMessage:
                return Color.cyan;
            default:
                return Color.white;
        }
    }

    private void PositionChats()
    {
        var parentRect = rectTransform.rect;

        float spacing = parentRect.height * 0.1f;
        float lastPosition = parentRect.height;
        for (int i = 0; i < sideChats.Count; i++)
        {
            var chat = sideChats[i];
            chat.label.rectTransform.anchoredPosition = new Vector3(spacing, lastPosition, 0);
            var size = chat.label.bounds.size.y;
            lastPosition += size + spacing;
        }
    }

    private void LateUpdate()
    {
        for (int i = 0; i < sideChats.Count; i++)
        {
            var chat = sideChats[i];
            chat.time += Time.deltaTime;
            if (chat.time > 14)
            {
                Destroy(chat.label.gameObject);

                sideChats.RemoveAt(i);
                i--;
            }
        }

    }
}
