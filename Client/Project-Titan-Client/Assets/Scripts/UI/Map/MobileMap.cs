using System;
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

public class MobileMap : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
#if UNITY_EDITOR
    , IDragHandler
#endif
{
    public World world;

    public RawImage image;

    public RawImage indicators;

    public Camera minimapCamera;

    public TooltipManager tooltipManager;

    private int tooltip = -1;

    private Texture2D mapTexture;

    private bool textureDirty = false;

    private float aspect;

    private bool inited = false;

    private float heightView = 70;

    private Vector2 mapPosition;

    private Vector2 lastCenter;

    private float lastDistance;

    private RenderTexture minimapRenderTexture;

    private float lastMapSize;

    private void Awake()
    {
        Init();
    }

    private void OnEnable()
    {
        if (world.player == null) return;
        SetMapPosition(world.player.Position, heightView);
    }

    private void Init()
    {
        if (inited) return;
        inited = true;

        var rect = GetComponent<RectTransform>().rect;
        aspect = rect.width / rect.height;

        CreateMinimapRenderTexture();
    }

    private void CreateMinimapRenderTexture()
    {
        var height = Mathf.Min(720, Screen.height);

        minimapRenderTexture = new RenderTexture(minimapCamera.targetTexture);
        minimapRenderTexture.height = height;
        minimapRenderTexture.width = (int)(height * aspect);
        minimapCamera.targetTexture = minimapRenderTexture;

        indicators.texture = minimapRenderTexture;
    }

    public void NewWorld(int width, int height)
    {
        Init();

        if (mapTexture != null)
            Destroy(mapTexture);

        mapTexture = new Texture2D(width, height);
        mapTexture.filterMode = FilterMode.Point;
        mapTexture.wrapMode = TextureWrapMode.Clamp;
        image.texture = mapTexture;

        var colors = new Color[width * height];
        for (int i = 0; i < width * height; i++)
            colors[i] = Color.clear;
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

    private void SetMapPosition(Vector2 position, float height)
    {
        mapPosition = position;
        heightView = height;

        minimapCamera.orthographicSize = heightView / 2;
        var mapPos = new Utils.NET.Geometry.Vec2(position.x, position.y).RotateOrigin(world.CameraRotation * Mathf.Deg2Rad);
        minimapCamera.transform.position = new Vector3(mapPos.x, mapPos.y, -10);
        minimapCamera.transform.localEulerAngles = new Vector3(0, 0, world.CameraRotation);

        position /= new Vector2(mapTexture.width, mapTexture.height);
        height /= mapTexture.height;
        image.uvRect = new Rect(position.x - height * aspect / 2, position.y - height / 2, height * aspect, height);

        SetAllIndicatorSizes();
    }

    private void SetAllIndicatorSizes()
    {
        var size = minimapCamera.orthographicSize;
        if (size == lastMapSize) return;
        lastMapSize = size;

        var objs = FindObjectsOfType<Indicator>();
        foreach (var obj in objs)
        {
            obj.SetSize(minimapCamera.orthographicSize * 0.7f);
        }
    }

    private void TranslateMap(Vector2 screenDelta)
    {
        var screenRelative = screenDelta / new Vector2(Screen.width, Screen.height);
        screenRelative.x *= aspect;
        var gameUnits = screenRelative * heightView;
        SetMapPosition(mapPosition - gameUnits, heightView);
    }

    private void LateUpdate()
    {
        UpdateTexture();

        CheckDragZoom();
    }

    private void UpdateTexture()
    {
        if (!textureDirty) return;
        textureDirty = false;
        mapTexture.Apply();
    }

    private void CheckDragZoom()
    {
#if UNITY_EDITOR
        var scroll = Input.mouseScrollDelta;
        if (scroll.y != 0)
        {
            heightView *= scroll.y < 0 ? 1.1f : 0.9f;
            SetMapPosition(mapPosition, heightView);
        }
#endif

        if (Input.touchCount == 1)
        {
            var touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Moved)
            {
                TranslateMap(touch.deltaPosition);
            }
        }
        else if (Input.touchCount != 0)
        {
            var touch1 = Input.GetTouch(0);
            var touch2 = Input.GetTouch(1);

            var center = touch1.position + (touch2.position - touch1.position) / 2;
            var distance = (touch1.position - touch2.position).magnitude;

            if (EligableTouch(touch1) && EligableTouch(touch2))
            {
                var scale = lastDistance / distance;
                heightView *= scale;

                SetMapPosition(mapPosition, heightView);
            }

            lastCenter = center;
            lastDistance = distance;
        }
    }

    private bool EligableTouch(Touch touch)
    {
        switch (touch.phase)
        {
            case TouchPhase.Moved:
            case TouchPhase.Stationary:
                return true;
            default:
                return false;
        }
    }

    private Vector2 downPosition;

    private float downTime;

    public void OnPointerDown(PointerEventData eventData)
    {
        downPosition = eventData.position;
        downTime = Time.time;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        var position = eventData.position;
        if (Time.time - downTime > 0.5f || Vector2.Distance(position, downPosition) > Screen.height * 0.05f) return;

        var rectTransform = GetComponent<RectTransform>();
        //var rect = rectTransform.rect;
        var relative = eventData.position - new Vector2(Screen.width / 2.0f, Screen.height / 2.0f);// - ((Vector2)rectTransform.position + rect.size / 2);
        //Debug.Log(relative);
        var worldPos = mapPosition + (relative / new Vector2(Screen.width, Screen.height)) * heightView;

        Character closest = null;
        float distance = heightView * 0.06f;
        foreach (var character in world.characters)
        {
            if (character is Player) continue;
            var dis = Vector2.Distance((Vector2)character.Position, worldPos);
            if (dis < distance)
            {
                closest = character;
                distance = dis;
            }
        }

        if (tooltip != -1)
        {
            tooltipManager.HideTooltip(tooltip);
        }

        if (closest != null)
            tooltip = tooltipManager.ShowTooltip(closest);
    }

#if UNITY_EDITOR

    public void OnDrag(PointerEventData eventData)
    {
        TranslateMap(eventData.delta);
    }

#endif
}
