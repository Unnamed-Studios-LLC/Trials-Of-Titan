using Assets.Scripts.MapEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TitanCore.Core;
using TitanCore.Data;
using TitanCore.Data.Items;
using TitanCore.Data.Map;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public enum MapEditorObjectType
{
    Object,
    Tile,
    Region
}

public class ObjectLayout : MonoBehaviour, IPointerDownHandler
{
    public World world;

    public Camera objectCamera;

    public GameObject spritePrefab;

    public GameObject meshPrefab;

    public GameObject regionPrefab;

    public RectTransform content;

    public RectTransform selection;

    public MapEditorObject selectedObject;

    public TMP_Dropdown dropdown;

    public MapEditorObjectType selectedType = MapEditorObjectType.Object;

    private MapEditorObject[] objs;

    private MapEditorObject[] tiles;

    private MapEditorRegion[] regions;

    private void Start()
    {
        LayoutAllObjects();

        dropdown.ClearOptions();
        foreach (var value in (MapEditorObjectType[])Enum.GetValues(typeof(MapEditorObjectType)))
            dropdown.options.Add(new TMP_Dropdown.OptionData(value.ToString() + 's'));

        SetSelectionType(MapEditorObjectType.Object);
        Select(0);
    }

    public void LayoutAllObjects()
    {
        LayoutObjects();
        LayoutTiles();
        LayoutRegions();
    }

    private void LayoutObjects()
    {
        var objectInfos = GameData.objects.Values.Where(_ => !(_ is TileInfo) && !(_ is ItemInfo) && !(_ is ProjectileInfo) && !_.serverOnly).ToArray();

        int index = 0;
        objs = new MapEditorObject[objectInfos.Length];
        foreach (var objInfo in objectInfos)
        {
            var obj = CreateObject(objInfo, true);
            obj.SetPosition(new Vector3(index % 3, -index / 3, -3));
            objs[index] = obj;
            obj.gameObject.SetActive(false);
            index++;
        }
    }

    private void LayoutTiles()
    {
        var objectInfos = GameData.objects.Values.Where(_ => _ is TileInfo && !_.serverOnly).ToArray();

        int index = 0;
        tiles = new MapEditorObject[objectInfos.Length];
        foreach (var objInfo in objectInfos)
        {
            var obj = CreateObject(objInfo, true);
            obj.SetPosition(new Vector2(index % 3, -index / 3));
            tiles[index] = obj;
            obj.gameObject.SetActive(false);
            index++;
        }
    }

    private void LayoutRegions()
    {
        var regionTypes = (Region[])Enum.GetValues(typeof(Region));

        int index = 0;
        regions = new MapEditorRegion[regionTypes.Length];
        foreach (var regionType in regionTypes)
        {
            var obj = CreateRegion(regionType);
            obj.SetPosition(new Vector2(index % 3, -index / 3));
            regions[index] = obj;
            obj.gameObject.SetActive(false);
            index++;
        }
    }

    public void DropdownChanged(int index)
    {
        SetSelectionType((MapEditorObjectType)index);
        Select(0);
    }

    public void SetSelectionType(MapEditorObjectType type)
    {
        foreach (var oldObj in ObjsForType(selectedType))
            oldObj.gameObject.SetActive(false);

        selectedType = type;
        var objs = ObjsForType(type);
        foreach (var obj in objs)
            obj.gameObject.SetActive(true);

        content.sizeDelta = new Vector2(content.sizeDelta.x, ((objs.Length + 2) / 3) * (objectCamera.targetTexture.height / (objectCamera.orthographicSize * 2)));
    }

    private MapEditorObject[] ObjsForType(MapEditorObjectType type)
    {
        switch (type)
        {
            case MapEditorObjectType.Tile:
                return tiles;
            case MapEditorObjectType.Region:
                return regions;
            default:
                return objs;
        }
    }

    public MapEditorObject CreateObject(GameObjectInfo info, bool forDisplay)
    {
        MapEditorObject obj;
        switch (info.Type)
        {
            case GameObjectType.Wall:
            case GameObjectType.Object3d:
                obj = Instantiate(meshPrefab).GetComponent<MapEditorMesh>();
                break;
            default:
                obj = Instantiate(spritePrefab).GetComponent<MapEditorSprite>();
                break;
        }
        obj.LoadInfo(info, forDisplay);
        return obj;
    }

    public MapEditorRegion CreateRegion(Region region)
    {
        var obj = Instantiate(regionPrefab).GetComponent<MapEditorRegion>();
        obj.SetRegion(region);
        return obj;
    }

    private void LateUpdate()
    {
        var screenPos = objectCamera.ScreenToViewportPoint(new Vector3(0, content.anchoredPosition.y, 0));
        objectCamera.transform.position = new Vector3(1.5f, -5 - screenPos.y * objectCamera.orthographicSize * 2, -5);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != 0) return;
        //var rectTransform = (RectTransform)transform;
        var rect = content.rect;
        rect.position = content.position - new Vector3(0, rect.height, 0);
        var pos = eventData.position - rect.position;
        if (pos.x < 0 || pos.y < 0 || pos.x >= rect.width || pos.y >= rect.height) return;
        pos.y -= rect.height;
        var objs = ObjsForType(selectedType);
        var cellSize = new Vector2(((RectTransform)transform).rect.width / 3, rect.height / ((objs.Length + 2) / 3));
        selection.sizeDelta = cellSize;
        int index = (int)(Mathf.Abs(pos.y) / cellSize.y) * 3 + (int)(pos.x / cellSize.x);
        if (index >= objs.Length) return;
        Select(index);
    }

    public void Select(int index)
    {
        switch (selectedType)
        {
            case MapEditorObjectType.Tile:
                SelectTile(ref index);
                break;
            case MapEditorObjectType.Region:
                SelectRegion(ref index);
                break;
            default:
                SelectObj(ref index);
                break;
        }

        var objPos = new Vector3(index % 3, index / 3) * selection.sizeDelta;
        objPos.y *= -1;
        selection.localPosition = objPos;
    }

    private void SelectObj(ref int index)
    {
        index = Mathf.Min(objs.Length - 1, index);
        var obj = objs[index];
        selectedObject = obj;
    }

    private void SelectTile(ref int index)
    {
        index = Mathf.Min(tiles.Length - 1, index);
        var obj = tiles[index];
        selectedObject = obj;
    }

    private void SelectRegion(ref int index)
    {
        index = Mathf.Min(regions.Length - 1, index);
        var obj = regions[index];
        selectedObject = obj;
    }
}
