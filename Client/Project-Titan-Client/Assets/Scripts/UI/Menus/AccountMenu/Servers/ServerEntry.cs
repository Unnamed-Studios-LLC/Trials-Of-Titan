using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Net.Web;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ServerEntry : MonoBehaviour
{
    public TextMeshProUGUI nameLabel;

    public TextMeshProUGUI statusLabel;

    public TextMeshProUGUI pingLabel;

    public Image pingImage;

    public Sprite badConnection;

    public Sprite okConnection;

    public Sprite goodConnection;

    public Color normalColor = Color.white;

    public Color crowdedColor = Color.white;

    public Color fullColor = Color.white;

    [HideInInspector]
    public WebServerInfo webInfo;

    private Ping ping;

    public void Setup(WebServerInfo info)
    {
        webInfo = info;

        nameLabel.text = info.name;
        statusLabel.text = info.status.ToString();
        statusLabel.color = ColorForStatus(info.status);

        pingLabel.gameObject.SetActive(false);
        pingImage.gameObject.SetActive(false);

        if (ping != null)
        {
            ping.DestroyPing();
        }
        ping = new Ping(info.pingHost);
    }

    public void SetupBest()
    {
        webInfo = null;

        nameLabel.text = "Best Server";
        statusLabel.gameObject.SetActive(false);
        pingLabel.gameObject.SetActive(false);
        pingImage.gameObject.SetActive(false);
    }

    public void SetSelected(bool selected)
    {
        var backImage = GetComponent<Image>();
        if (selected)
            backImage.color = Color.white;
        else
            backImage.color = new Color(0.5f, 0.5f, 0.5f, 1);
    }

    private Color ColorForStatus(ServerStatus status)
    {
        switch (status)
        {
            case ServerStatus.Full:
                return fullColor;
            case ServerStatus.Crowded:
                return crowdedColor;
            default:
                return normalColor;
        }
    }

    private void OnDisable()
    {
        if (ping == null) return;
        ping.DestroyPing();
        ping = null;
    }

    private void LateUpdate()
    {
        UpdatePing();
    }

    private void UpdatePing()
    {
        if (pingLabel.gameObject.activeSelf) return;
        if (ping == null) return;
        if (!ping.isDone) return;

        pingLabel.text = $"{ping.time}ms";
        pingImage.sprite = SpriteForPing(ping.time);

        pingLabel.gameObject.SetActive(true);
        pingImage.gameObject.SetActive(true);
    }

    private Sprite SpriteForPing(int time)
    {
        if (time < 100)
            return goodConnection;
        else if (time < 180)
            return okConnection;
        else
            return badConnection;
    }
}
