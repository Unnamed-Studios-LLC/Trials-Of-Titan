using Mobile;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TitanCore.Net.Packets.Models;
using TitanCore.Net.Packets.Server;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MobileGameUI : GameUI
{
    private RenderTexture gameRenderTexture;

    public RawImage gameView;

    public NearbyPlayerMobile[] nearbyPlayers;

    public MobileMap map;

    public SideMenuManager sideMenuManager;

    public OptionsMenu optionsMenu;

    public LootMenu lootMenu;

    public TextMeshProUGUI playerName;

    public TextMeshProUGUI levelLabel;

    private void Awake()
    {
        MakeRenderTexture();
    }

    public override void NewWorld(TnMapInfo mapInfo)
    {
        base.NewWorld(mapInfo);

        map.NewWorld(mapInfo.width, mapInfo.height);
    }

    private void MakeRenderTexture()
    {
        int targetSize = 800;
        int scale = Mathf.Max(1, Screen.height / targetSize);
        targetSize *= scale;

        gameRenderTexture = new RenderTexture(gameManager.world.worldCamera.targetTexture);
        gameRenderTexture.height = targetSize;
        gameRenderTexture.width = (int)(targetSize * (Screen.width / (float)Screen.height));
        gameManager.world.worldCamera.targetTexture = gameRenderTexture;

        gameView.texture = gameRenderTexture;
        gameManager.world.worldCamera.GetComponent<ObliqueProjection>().Apply();
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();

    }

    public override void WorldLoaded()
    {
        base.WorldLoaded();

        playerName.text = gameManager.world.player.playerName;
    }

    private void UpdateNearbyPlayers()
    {
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

    public override void ShowContainer(IContainer container)
    {
        lootMenu.Show();
        base.ShowContainer(container);
    }

    public override void HideContainer()
    {
        lootMenu.Hide();
        base.HideContainer();
    }

    public override void PostLateUpdate()
    {
        UpdateNearbyPlayers();
    }

    public override void TileDiscovered(MapTile tile)
    {
        map.TileDiscovered(tile);
    }

    public override void PlayerDamageTaken()
    {
        sideMenuManager.DamageTaken();
        optionsMenu.Close();
    }

    protected override int GetOutlineThinkness(float screenHeight)
    {
        return 1 + (int)(screenHeight / 500);
    }

    public override void OnPlayerStatsUpdated(Player player)
    {
        base.OnPlayerStatsUpdated(player);

        levelLabel.text = "Level " + player.GetLevel();
    }
}
