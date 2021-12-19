using System.Collections;
using System.Collections.Generic;
using TitanCore.Net.Packets.Models;
using TitanCore.Net.Packets.Server;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TradeMenu : MonoBehaviour
{
    private static Color defaultInfoColor = new Color(0.7294118f, 0.7215686f, 0.7450981f);
    private static Color acceptedInfoColor = Color.green;

    private const float Accept_Delay = 2;

    public TextMeshProUGUI playerName;

    public TextMeshProUGUI playerInfo;

    public bool includeEquips = true;

    public TradeSlot[] playerSlots;

    public TextMeshProUGUI otherName;

    public TextMeshProUGUI otherInfo;

    public TradeSlot[] otherSlots;

    public Slider acceptDelay;

    public Button acceptButton;

    private Player player;

    private int tradeVersion = 0;

    private TradeOffer lastOffer;

    private float lastChangeTime;

    public void StartTrade(TnTradeStart start, Character other, Player player)
    {
        this.player = player;
        gameObject.SetActive(true);

        playerName.text = player.playerName;

        otherName.text = other.playerName;

        if (includeEquips)
        {
            for (int i = 0; i < playerSlots.Length; i++)
                playerSlots[i].SetItem(player, player.GetItem(i), i, true);

            for (int i = 0; i < 4; i++)
                otherSlots[i].SetItem(other, other.GetItem(i), i, false);
            for (int i = 4; i < 12; i++)
                otherSlots[i].SetItem(other, start.items[i - 4], i, false);
        }
        else
        {
            for (int i = 0; i < 8; i++)
                playerSlots[i].SetItem(player, player.GetItem(i + 4), i + 4, true);
            for (int i = 0; i < 8; i++)
                otherSlots[i].SetItem(other, start.items[i], i + 4, false);
        }

        foreach (var slot in playerSlots)
            slot.Selected = false;
        foreach (var slot in otherSlots)
            slot.Selected = false;

        SetPlayerAccepted(false);
        SetOtherAccepted(false);

        ResetAcceptDelay();
    }

    private void ResetAcceptDelay()
    {
        lastChangeTime = Time.time;
    }

    private void SetPlayerAccepted(bool accepted)
    {
        if (accepted)
        {
            playerInfo.text = "You have accepted!";
            playerInfo.color = acceptedInfoColor;
            playerName.color = acceptedInfoColor;
        }
        else
        {
            playerInfo.text = "Select items you want to trade";
            playerInfo.color = defaultInfoColor;
            playerName.color = defaultInfoColor;
        }
    }

    private void SetOtherAccepted(bool accepted)
    {
        if (accepted)
        {
            otherInfo.text = "Player has accepted!";
            otherInfo.color = acceptedInfoColor;
            otherName.color = acceptedInfoColor;
        }
        else
        {
            otherInfo.text = "Player is selecting items";
            otherInfo.color = defaultInfoColor;
            otherName.color = defaultInfoColor;
        }
    }

    public void UpdateTrade(TnTradeUpdate update)
    {
        if (includeEquips)
        {
            for (int i = 4; i < otherSlots.Length; i++)
            {
                var slot = otherSlots[i];
                slot.Selected = update.offer[i - 4];
            }
        }
        else
        {
            for (int i = 0; i < otherSlots.Length; i++)
            {
                var slot = otherSlots[i];
                slot.Selected = update.offer[i];
            }
        }
        tradeVersion = update.version;

        SetOtherAccepted(update.accepted);
        SetPlayerAccepted(false);

        ResetAcceptDelay();
    }

    private TradeOffer GetOffer()
    {
        var offer = new TradeOffer();
        if (includeEquips)
        {
            for (int i = 0; i < 8; i++)
                offer[i] = playerSlots[i + 4].Selected;
        }
        else
        {
            for (int i = 0; i < 8; i++)
                offer[i] = playerSlots[i].Selected;
        }
        return offer;
    }

    private void LateUpdate()
    {
        var offer = GetOffer();
        if (offer != lastOffer)
        {
            SetPlayerAccepted(false);
            player.world.gameManager.client.SendAsync(new TnTradeUpdate(tradeVersion, false, offer));
            lastOffer = offer;

            ResetAcceptDelay();
        }

        acceptButton.interactable = CanAccept();
        acceptDelay.value = Mathf.Clamp01((Time.time - lastChangeTime) / Accept_Delay);
    }

    private bool CanAccept()
    {
        return Time.time - lastChangeTime > Accept_Delay;
    }

    public void Accept()
    {
        if (!CanAccept()) return;
        player.world.gameManager.client.SendAsync(new TnTradeUpdate(tradeVersion, true, GetOffer()));
        SetPlayerAccepted(true);
    }

    public void Cancel()
    {
        player.world.gameManager.client.SendAsync(new TnTradeResult(TradeResult.Cancelled));
        gameObject.SetActive(false);
    }
}
