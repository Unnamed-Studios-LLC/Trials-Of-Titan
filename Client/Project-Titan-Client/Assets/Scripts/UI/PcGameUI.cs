using Pc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Net.Packets.Models;
using TitanCore.Net.Packets.Server;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PcGameUI : GameUI
{
    public Image playerPreview;

    public TextMeshProUGUI playerName;

    public TextMeshProUGUI levelLabel;

    public Minimap minimap;

    public NearbyPlayer[] nearbyPlayers;

    public TextMeshProUGUI worldLabel;

    private string worldName;

    private int worldMaxCount;

    public Image cooldownIndicator;

    public override void NewWorld(TnMapInfo mapInfo)
    {
        base.NewWorld(mapInfo);

        worldName = mapInfo.worldName;
        worldMaxCount = mapInfo.maxPlayerCount;
        minimap.NewWorld(mapInfo.width, mapInfo.height);
    }

    public override void WorldLoaded()
    {
        base.WorldLoaded();

        UpdatePlayerPreview();
    }

    private void UpdatePlayerPreview()
    {
        var previewSprite = TextureManager.GetDisplaySprite(gameManager.world.player.GetSkinInfo());
        playerPreview.sprite = previewSprite;

        float aspect = previewSprite.textureRect.width / previewSprite.textureRect.height;
        playerPreview.rectTransform.anchorMax = new Vector2(playerPreview.rectTransform.anchorMin.x + 0.16666662f * aspect, playerPreview.rectTransform.anchorMax.y);
        playerPreview.rectTransform.offsetMax = new Vector2(0, 0);

        playerName.rectTransform.anchorMin = new Vector2(playerPreview.rectTransform.anchorMax.x + 0.0355556f, playerName.rectTransform.anchorMin.y);
        playerName.rectTransform.offsetMax = new Vector2(0, 0);

        playerName.text = gameManager.world.player.playerName;
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();

        UpdateNearbyPlayers();

        if (gameManager.world.player != null)
        {
            cooldownIndicator.fillAmount = 1.0f - Mathf.Clamp01(gameManager.world.player.cooldown / (float)gameManager.world.player.cooldownDuration);
        }
    }

    private void UpdateNearbyPlayers()
    {
        if (worldLabel != null)
            worldLabel.text = $"{worldName} ({gameManager.world.characters.Count}/{worldMaxCount})";

        var charactersSorted = gameManager.world.characters.OrderBy(_ => ((Vector2)_.Position - (Vector2)gameManager.world.player.Position).magnitude);
        int index = 0;
        foreach (var character in charactersSorted)
        {
            if (character is Player) continue;
            if (index >= nearbyPlayers.Length) break;
            nearbyPlayers[index++].SetCharacter(character);
        }

        for (int i = index; i < nearbyPlayers.Length; i++)
            nearbyPlayers[i].SetCharacter(null);
    }

    public override void TileDiscovered(MapTile tile)
    {
        minimap.TileDiscovered(tile);
    }

    public override void PostLateUpdate()
    {
        minimap.PostLateUpdate();
    }

    public override void PlayerDamageTaken()
    {

    }

    public override void OnPlayerStatsUpdated(Player player)
    {
        base.OnPlayerStatsUpdated(player);

        levelLabel.text = "Level " + player.GetLevel();
        UpdatePlayerPreview();
    }

    protected override int GetOutlineThinkness(float screenHeight)
    {
        return 1 + (int)(screenHeight / 1200);
    }
}
