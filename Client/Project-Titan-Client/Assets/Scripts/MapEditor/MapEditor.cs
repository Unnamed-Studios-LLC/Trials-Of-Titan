using Assets.Scripts.MapEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Core;
using TitanCore.Data;
using TitanCore.Data.Entities;
using TitanCore.Files;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using Utils.NET.Geometry;
using static TitanCore.Files.MapElementFile;

public class MapEditor : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public Camera worldCamera;

    public ObjectLayout objLayout;

    public MapEditorObject[,] objects;

    public MapEditorObject[,] tiles;

    public MapEditorRegion[,] regions;

    public MapEditorTool[] toolsArray;

    public TileRotation tileRotation;

    public MapEditorTool currentTool;

    private Dictionary<MapEditorToolType, MapEditorTool> tools;

    private int width = 1000;

    private int height = 1000;

    private void Start()
    {
        objects = new MapEditorObject[width, height];
        tiles = new MapEditorObject[width, height];
        regions = new MapEditorRegion[width, height];

        tools = toolsArray.ToDictionary(_ => _.Type);
        SetTool(MapEditorToolType.Draw);
    }

    private MapEditorObject[,] GetObjs(MapEditorObjectType type)
    {
        switch (type)
        {
            case MapEditorObjectType.Tile:
                return tiles;
            case MapEditorObjectType.Region:
                return regions;
            default:
                return objects;
        }
    }

    public void Export()
    {
#if UNITY_EDITOR
        var filePath = EditorUtility.SaveFilePanel("Export Map", null, null, "mef");
        if (string.IsNullOrWhiteSpace(filePath)) return;
        var map = ExportMap();
        using (var stream = new MemoryStream())
        {
            map.Write(stream);
            File.WriteAllBytes(filePath, stream.ToArray());
        }
#endif
    }

    private MapElementFile ExportMap()
    {
        var bl = new Int2(width, height);
        var tr = new Int2(0, 0);

        for (int y = 0; y < height; y++) // get bounds
            for (int x = 0; x < width; x++)
            {
                var tile = tiles[x, y];
                var obj = objects[x, y];
                var region = regions[x, y];
                if (tile == null && obj == null && region == null) continue;
                if (x < bl.x)
                    bl.x = x;
                if (x > tr.x)
                    tr.x = x;

                if (y < bl.y)
                    bl.y = y;
                if (y > tr.y)
                    tr.y = y;
            }

        var file = new MapElementFile();
        file.width = tr.x - bl.x + 1;
        file.height = tr.y - bl.y + 1;

        var tileElements = new MapTileElement[file.width, file.height];
        for (int y = 0; y < file.height; y++) // get bounds
            for (int x = 0; x < file.width; x++)
            {
                var tile = tiles[bl.x + x, bl.y + y];
                var obj = objects[bl.x + x, bl.y + y];
                tileElements[x, y] = new MapTileElement()
                {
                    tileType = tile == null ? (ushort)0 : tile.info.id,
                    objectType = obj == null ? (ushort)0 : obj.info.id
                };
            }
        file.tiles = tileElements;
        file.entities = new MapEntityElement[0];

        var regionList = new List<MapRegionElement>();
        for (int y = 0; y < file.height; y++)
            for (int x = 0; x < file.width; x++)
            {
                var region = regions[bl.x + x, bl.y + y];
                if (region == null) continue;
                regionList.Add(new MapRegionElement()
                {
                    x = (uint)x,
                    y = (uint)y,
                    regionType = region.region
                });
            }
        file.regions = regionList.ToArray();

        return file;
    }

    public void Import()
    {
#if UNITY_EDITOR
        var filePath = EditorUtility.OpenFilePanel("Import Map", null, "mef");
        if (string.IsNullOrWhiteSpace(filePath)) return;
        var file = MapElementFile.ReadFrom(filePath);
        Clear();
        ImportMap(file);
#endif
    }

    private void ImportMap(MapElementFile file)
    {
        width = Math.Max(1000, file.width);
        height = Math.Max(1000, file.height);

        var bl = new Int2(width / 2 - file.width / 2, height / 2 - file.height / 2);
        for (int y = 0; y < file.height; y++)
            for (int x = 0; x < file.width; x++)
            {
                var tile = file.tiles[x, y];
                if (tile.tileType != 0)
                    CreateObject(bl.x + x, bl.y + y, GameData.objects[tile.tileType], MapEditorObjectType.Tile);
                if (tile.objectType != 0)
                    CreateObject(bl.x + x, bl.y + y, GameData.objects[tile.objectType], MapEditorObjectType.Object);
            }

        foreach (var region in file.regions)
        {
            CreateRegion(bl.x + (int)region.x, bl.y + (int)region.y, region.regionType, MapEditorObjectType.Region);
        }

        worldCamera.transform.position = new Vector3(width / 2, height / 2, worldCamera.transform.position.z);
    }

    public void Clear()
    {
        for (int y = 0; y < width; y++) // get bounds
            for (int x = 0; x < height; x++)
            {
                var tile = tiles[x, y];
                if (tile != null)
                    Destroy(tile.gameObject);

                var obj = objects[x, y];
                if (obj != null)
                    Destroy(obj.gameObject);

                var region = regions[x, y];
                if (region != null)
                    Destroy(region.gameObject);
            }

        width = 1000;
        height = 1000;
        objects = new MapEditorObject[width, height];
        tiles = new MapEditorObject[width, height];
        regions = new MapEditorRegion[width, height];
    }

    public void SetDraw()
    {
        SetTool(MapEditorToolType.Draw);
    }

    public void SetErase()
    {
        SetTool(MapEditorToolType.Erase);
    }

    public void SetFill()
    {
        SetTool(MapEditorToolType.Fill);
    }

    public void SetCircle()
    {
        SetTool(MapEditorToolType.Circle);
    }

    public void SetTool(MapEditorToolType type)
    {
        if (currentTool != null && currentTool.Type == type) return;
        if (!tools.TryGetValue(type, out var newTool))
            return;
        currentTool?.SetNormal();
        currentTool = newTool;
        currentTool.SetSelected();
    }

    private Int2 GetWorldPositionFromMouse()
    {
        var mousePos = Input.mousePosition;
        var pos = worldCamera.ScreenToWorldPoint(mousePos);
        return new Int2((int)pos.x, (int)pos.y);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != 0) return;
        var pos = GetToolPosition();
        currentTool?.StartTool(pos.x, pos.y);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button != 0) return;
        var pos = GetToolPosition();
        currentTool?.DoTool(pos.x, pos.y);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button != 0) return;
        var pos = GetToolPosition();
        currentTool?.EndTool(pos.x, pos.y);
    }

    private Int2 GetToolPosition()
    {
        var pos = GetWorldPositionFromMouse();
        var objs = GetObjs(objLayout.selectedType);
        var intPos = new Int2(pos.x, pos.y);
        intPos.x = Mathf.Min(Mathf.Max(intPos.x, 0), objs.GetLength(0) - 1);
        intPos.y = Mathf.Min(Mathf.Max(intPos.y, 0), objs.GetLength(1) - 1);
        return intPos;
    }

    public void Fill(int x, int y, MapEditorObjectType sampleFrom, bool clear)
    {
        if (x < 0 || y < 0 || x >= width || y >= height) return;

        var sampleGroup = GetObjs(sampleFrom);

        ushort toFillType = sampleGroup[x, y]?.Id ?? 0;

        if (toFillType == objLayout.selectedObject.Id) return;

        var filled = new HashSet<Int2>();
        var toProcess = new Queue<Int2>();
        toProcess.Enqueue(new Int2(x, y));

        while (toProcess.Count > 0)
        {
            var point = toProcess.Dequeue();
            if (point.x < 0 || point.y < 0 || point.x >= width || point.y >= height || filled.Contains(point)) continue;

            var objId = sampleGroup[point.x, point.y]?.Id ?? 0;
            if (objId == toFillType)
            {
                if (clear)
                    EraseObject(point.x, point.y);
                else
                    CreateObject(point.x, point.y);

                filled.Add(point);

                toProcess.Enqueue(new Int2(point.x + 1, point.y));
                toProcess.Enqueue(new Int2(point.x - 1, point.y));
                toProcess.Enqueue(new Int2(point.x, point.y + 1));
                toProcess.Enqueue(new Int2(point.x, point.y - 1));
            }
        }
    }

    public void CreateObject(int x, int y)
    {
        if (x < 0 || y < 0 || x >= width || y >= height) return;
        if (objLayout.selectedType == MapEditorObjectType.Region)
            CreateRegion(x, y, ((MapEditorRegion)objLayout.selectedObject).region, MapEditorObjectType.Region);
        else
            CreateObject(x, y, objLayout.selectedObject.info, objLayout.selectedType);
    }

    private void CreateObject(int x, int y, GameObjectInfo info, MapEditorObjectType type)
    {
        DestroyObject(x, y, type);

        var obj = objLayout.CreateObject(info, false);

        SetupObject(x, y, obj, type);
    }

    private void CreateRegion(int x, int y, Region region, MapEditorObjectType type)
    {
        DestroyObject(x, y, type);

        var obj = objLayout.CreateRegion(region);

        SetupObject(x, y, obj, type);
    }

    private void DestroyObject(int x, int y, MapEditorObjectType type)
    {
        var objs = GetObjs(type);
        var currentObj = objs[x, y];
        if (currentObj != null)
        {
            Destroy(currentObj.gameObject);
        }
    }

    private void SetupObject(int x, int y, MapEditorObject obj, MapEditorObjectType type)
    {
        var offset = new Vector2(0, 1);
        if (type == MapEditorObjectType.Tile)
        {
            obj.transform.localEulerAngles = new Vector3(0, 0, (int)tileRotation * 90);
            switch (tileRotation)
            {
                case TileRotation.Quarter:
                    offset = new Vector2(0, 0);
                    break;
                case TileRotation.Half:
                    offset = new Vector2(1, 0);
                    break;
                case TileRotation.ThreeQuarters:
                    offset = new Vector2(1, 1);
                    break;
            }
        }

        obj.SetPosition(new Vector3(x + offset.x, y + offset.y, DepthOfType(type)));
        obj.gameObject.layer = 0;
        for (int i = 0; i < obj.transform.childCount; i++)
            obj.transform.GetChild(i).gameObject.layer = 0;
        var objs = GetObjs(type);
        objs[x, y] = obj;
    }

    public void EraseObject(int x, int y)
    {
        if (x < 0 || y < 0 || x >= width || y >= height) return;
        var objs = GetObjs(objLayout.selectedType);
        var currentObj = objs[x, y];
        if (currentObj == null) return;
        Destroy(currentObj.gameObject);
        objs[x, y] = null;
    }

    private int DepthOfType(MapEditorObjectType type)
    {
        switch (type)
        {
            case MapEditorObjectType.Tile:
                return -1;
            case MapEditorObjectType.Region:
                return -9;
            default:
                return -2;
        }
    }

    private void LateUpdate()
    {
        foreach (var tool in toolsArray)
        {
            if (Input.GetKeyDown(tool.selectKey))
            {
                SetTool(tool.Type);
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            tileRotation = (TileRotation)(((int)tileRotation + 1) % 4);
        }

        if (currentTool is CircleTool circleTool)
        {
            if (Input.GetKeyDown(KeyCode.Equals) || Input.GetKey(KeyCode.Equals))
            {
                circleTool.diameter++;
            }
            if (Input.GetKeyDown(KeyCode.Minus))
            {
                circleTool.diameter = Mathf.Max(1, circleTool.diameter - 1);
            }
            if (Input.GetKeyDown(KeyCode.M))
            {
                circleTool.clearMode = !circleTool.clearMode;
            }
        }

        if (currentTool is FillTool fillTool)
        {
            if (Input.GetKeyDown(KeyCode.N))
            {
                fillTool.IncrementSampleType();
            }
            if (Input.GetKeyDown(KeyCode.M))
            {
                fillTool.clearMode = !fillTool.clearMode;
            }
        }
    }
}