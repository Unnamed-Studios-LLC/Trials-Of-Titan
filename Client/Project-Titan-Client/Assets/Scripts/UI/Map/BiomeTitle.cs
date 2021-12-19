using System.Collections;
using System.Collections.Generic;
using TitanCore.Net.Packets.Server;
using TMPro;
using UnityEngine;
using Utils.NET.Utils;

public class BiomeTitle : MonoBehaviour
{
    private enum BiomeArea
    {
        None,
        SettingShores,
        MysticalMeadows,
        WeepingWilderness,
        DesolateDunes,
        SanguineShallows,
        PerilousPeaks,
        TreacherousTundra
    }

    private static Dictionary<ushort, BiomeArea> biomeAreaTiles = new Dictionary<ushort, BiomeArea>
    {
        { 0xb03, BiomeArea.SettingShores },
        { 0xb04, BiomeArea.SettingShores },

        { 0xb02, BiomeArea.MysticalMeadows },

        { 0xb05, BiomeArea.WeepingWilderness },
        { 0xb06, BiomeArea.WeepingWilderness },

        { 0xb07, BiomeArea.DesolateDunes },
        { 0xb08, BiomeArea.DesolateDunes },
        { 0xb29, BiomeArea.DesolateDunes },

        { 0xb24, BiomeArea.SanguineShallows },
        { 0xb25, BiomeArea.SanguineShallows },
        { 0xb26, BiomeArea.SanguineShallows },
        { 0xb27, BiomeArea.SanguineShallows },
        { 0xb28, BiomeArea.SanguineShallows },

        { 0xb0d, BiomeArea.PerilousPeaks },
        { 0xb0e, BiomeArea.PerilousPeaks },

        { 0xb1e, BiomeArea.TreacherousTundra },
        { 0xb1f, BiomeArea.TreacherousTundra },
        { 0xb20, BiomeArea.TreacherousTundra },
        { 0xb21, BiomeArea.TreacherousTundra },
        { 0xb22, BiomeArea.TreacherousTundra },
        { 0xb23, BiomeArea.TreacherousTundra },
    };

    private const float Cooldown = 3f;

    public World world;

    private BiomeArea currentArea = BiomeArea.None;

    private BiomeArea targetArea = BiomeArea.None;

    private TextMeshProUGUI label;

    private float delay = 0;

    private TnMapInfo mapInfo;

    private void Awake()
    {
        label = GetComponent<TextMeshProUGUI>();
    }

    private void LateUpdate()
    {
        if (world.mapInfo != mapInfo)
        {
            mapInfo = world.mapInfo;
            label.text = mapInfo.worldName;
            ShowTitle();
        }

        if (world.player == null || !world.dynamicMusic) return;

        var newArea = GetCurrentArea();
        if (newArea != BiomeArea.None && newArea != targetArea)
        {
            targetArea = newArea;
            delay = Cooldown;
        }

        delay -= Time.deltaTime;
        if (targetArea != BiomeArea.None && targetArea != currentArea && delay <= 0)
        {
            currentArea = targetArea;
            label.text = StringUtils.Labelize(currentArea.ToString());
            ShowTitle();

            targetArea = BiomeArea.None;
        }
    }

    private BiomeArea GetCurrentArea()
    {
        var position = world.player.Position;
        var tile = world.tilemapManager.GetTileType((int)position.x, (int)position.y);
        if (biomeAreaTiles.TryGetValue(tile, out var areaType))
            return areaType;
        return BiomeArea.None;
    }

    private const float Fade_In_Time = 1.8f;
    private const float Hold_Time = 2.0f;
    private const float Fade_Out_Time = 0.6f;

    private void ShowTitle()
    {
        gameObject.LeanCancel();
        var rectTransform = (RectTransform)transform;
        rectTransform.anchoredPosition = Vector2.zero;
        transform.FadeInMoveUp(Fade_In_Time, 0, true);
        var seq = LeanTween.sequence().append(Fade_In_Time + Hold_Time).append(() =>
        {
            if (this == null || gameObject == null || !gameObject) return;
            gameObject.FadeOut(Fade_Out_Time, 0);
        });
    }
}
