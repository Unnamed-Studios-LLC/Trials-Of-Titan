using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Data;
using TitanCore.Data.Map;
using TitanCore.Net.Packets.Models;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Minimap : MonoBehaviour, IPointerClickHandler
{
    private Texture2D mapTexture;

    public RawImage image;

    public World world;

    private bool textureDirty = false;

    private int mapSize = 30;

    private int lastSize = -1;

    public bool mapCentered = false;

    public Camera minimapCamera;

    public TeleportPanel teleportPanel;

    private Option zoomInKey;

    private Option zoomOutKey;

    private void Awake()
    {
        minimapCamera.orthographicSize = mapSize;

        zoomInKey = Options.Get(OptionType.MinimapZoomIn);
        zoomOutKey = Options.Get(OptionType.MinimapZoomOut);
    }

    public void NewWorld(int width, int height)
    {
        if (mapTexture != null)
            Destroy(mapTexture);

        mapTexture = new Texture2D(width, height);
        mapTexture.filterMode = FilterMode.Point;
        mapTexture.wrapMode = TextureWrapMode.Clamp;
        image.texture = mapTexture;

        var colors = new Color[width * height];
        for (int i = 0; i < width * height; i++)
            colors[i] = Color.black;
        mapTexture.SetPixels(colors);
        mapTexture.Apply();
    }

    public void TileDiscovered(MapTile tile)
    {
        ushort displayType = tile.tileType;
        if (tile.objectType > 0)
        {
            var obj = GameData.objects[tile.objectType];
            if ((obj is Object3dInfo obj3d && obj3d.meshNames.Length > 0) || (obj is StaticObjectInfo staticInfo && staticInfo.collidable))
                displayType = tile.objectType;
        }

        if (displayType == 0)
            mapTexture.SetPixel(tile.x, tile.y, Color.black);
        else
        {
            var info = GameData.objects[displayType];
            if (info is ContextualWallInfo contextualInfo)
            {
                var meta = MeshManager.GetMesh(contextualInfo.meshNames[0] + "-side");
                mapTexture.SetPixel(tile.x, tile.y, meta.meanColor);
            }
            else if (info is Object3dInfo obj3dInfo)
            {
                var meta = MeshManager.GetMesh(obj3dInfo.meshNames[0]);
                mapTexture.SetPixel(tile.x, tile.y, meta.meanColor);
            }
            else
            {
                var meta = TextureManager.GetMetaData(TextureManager.GetSprite(info.textures[0].displaySprite));
                mapTexture.SetPixel(tile.x, tile.y, meta.averageColor);
            }
        }
        textureDirty = true;
    }

    public void PostLateUpdate()
    {
        UpdateTexture();

        PositionToPlayer();

        SetAllIndicatorSizes();

        CheckZoomKeys();
    }

    private void CheckZoomKeys()
    {
        if (Input.GetKeyDown(zoomInKey.GetKey()))
        {
            ZoomIn();
        }

        if (Input.GetKeyUp(zoomOutKey.GetKey()))
        {
            ZoomOut();
        }
    }

    private void UpdateTexture()
    {
        if (!textureDirty) return;
        textureDirty = false;
        mapTexture.Apply();
    }

    private void PositionToPlayer()
    {
        if (world.player == null) return;

        var pos = mapCentered ? new Vector3(mapTexture.width / 2, mapTexture.height / 2, 0) : world.player.Position;

        image.uvRect = new Rect((pos.x - mapSize) / mapTexture.width, (pos.y - mapSize) / mapTexture.height, (mapSize * 2) / (float)mapTexture.width, (mapSize * 2) / (float)mapTexture.height);
        minimapCamera.orthographicSize = mapSize;

        if (mapCentered)
        {
            var rotated = new Utils.NET.Geometry.Vec2(pos.x, pos.y).RotateOrigin(world.CameraRotation * Mathf.Deg2Rad);
            minimapCamera.transform.position = new Vector3(rotated.x, rotated.y, -10);
            minimapCamera.transform.localEulerAngles = new Vector3(0, 0, world.CameraRotation);
        }
        else
        {
            minimapCamera.transform.position = world.worldCamera.transform.position;
            minimapCamera.transform.localEulerAngles = new Vector3(0, 0, world.CameraRotation);
        }
    }

    public void ZoomIn()
    {
        var scale = Mathf.Max(15, (int)(mapSize * 0.7f));
        mapSize = scale;

        mapCentered = false;
    }

    public void ZoomOut()
    {
        var min = Mathf.CeilToInt(Mathf.Max(mapTexture.width / 2f, mapTexture.height / 2f));
        var scale = Mathf.Min(min, (int)(mapSize * 1.3f));
        mapSize = scale;

        mapCentered = mapSize == min;
    }

    private void SetAllIndicatorSizes()
    {
        if (mapSize == lastSize) return;
        lastSize = mapSize;

        var objs = FindObjectsOfType<Indicator>();
        foreach (var obj in objs)
        {
            obj.SetSize(minimapCamera.orthographicSize);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        var rectTransform = GetComponent<RectTransform>();
        var rect = rectTransform.rect;
        var relative = eventData.position - ((Vector2)rectTransform.position + rect.size / 2);
        relative = relative.ToVec2().RotateOrigin(world.CameraRotation * Mathf.Deg2Rad).ToVector2();
        var worldPos = (Vector2)minimapCamera.transform.position + relative * ((minimapCamera.orthographicSize * 2) / rect.size.y);
        teleportPanel.Show(worldPos, mapSize * 0.4f);
        teleportPanel.transform.position = eventData.position - new Vector2(5, 0);
    }
}
