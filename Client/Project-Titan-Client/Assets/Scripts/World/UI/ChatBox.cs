using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class ChatBox : MonoBehaviour
{
    public TextMeshProUGUI label;

    public RectTransform rect;

    public WorldObject owner;

    public RectTransform gameView;

    public Image background;

    public Image arrow;

    public Sprite normalBackground;
    public Sprite normalArrow;

    public Sprite enemyBackground;
    public Sprite enemyArrow;

    public Sprite guildBackground;
    public Sprite guildArrow;

    public float time;

    private static Color defaultColor = new Color(0.2209861f, 0.2209861f, 0.245283f, 1);

#if UNITY_IOS || UNITY_ANDROID
    private void Awake()
    {
        label.fontSize = Screen.height * 0.02f;
        var size = label.rectTransform.sizeDelta;
        size.x = Screen.height * 0.26f;
        label.rectTransform.sizeDelta = size;
    }
#endif

    public void SetGameView(RectTransform gameView)
    {
        this.gameView = gameView;
    }

    public void SetText(string text, WorldObject obj)
    {
        label.SetText(CleanText(text));
        label.ForceMeshUpdate();
        var bounds = label.textBounds;

        var rect = GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(Mathf.Max(22, bounds.size.x + 18), Mathf.Max(28, bounds.size.y + 16));

        if (obj is NotPlayable)
        {
            background.sprite = enemyBackground;
            arrow.sprite = enemyArrow;
            label.color = Color.white;
        }
        else
        {
            background.sprite = normalBackground;
            arrow.sprite = normalArrow;
            label.color = defaultColor;
        }
    }

    public static string CleanText(string text)
    {
        var builder = new StringBuilder();
        bool keep = true;
        for (int i = 0; i < text.Length; i++)
        {
            var c = text[i];
            if (c == '<')
                keep = false;
            if (keep)
                builder.Append(c);
            if (c == '>')
                keep = true;
        }
        return builder.ToString();
    }

    private void PositionToOwner()
    {
#if UNITY_IOS || UNITY_ANDROID
        var position = owner.transform.position + new Vector3(0, 0, -owner.GetHeight() - 0.25f);
        var screenPos = owner.world.worldCamera.WorldToScreenPoint(position);// * new Vector3(Screen.height / (float)owner.world.worldCamera.targetTexture.height, Screen.height / (float)owner.world.worldCamera.targetTexture.height, 1);
        screenPos.x *= Screen.height / (float)owner.world.worldCamera.targetTexture.height;
        screenPos.y *= Screen.width / (float)owner.world.worldCamera.targetTexture.width;
#else
        var position = owner.transform.position + new Vector3(0, 0, -owner.GetHeight() - 0.25f);
        var screenPos = owner.world.worldCamera.WorldToScreenPoint(position);
#endif
        //var screenPos = new Vector3((viewportPosition.x / Screen.width) * gameView.rect.width, (viewportPosition.y / Screen.height) * gameView.rect.height, 0);
        rect.anchoredPosition = screenPos;
    }

#if UNITY_EDITOR

    private string lastText;

    private void LateUpdate()
    {
        if (label.text != lastText)
        {
            SetText(label.text, owner);
            lastText = label.text;
        }

        if (owner == null)
            owner = FindObjectOfType<World>().player;
        else
            PositionToOwner();
    }

#else

    private void LateUpdate()
    {
        PositionToOwner();
    }

#endif
}
