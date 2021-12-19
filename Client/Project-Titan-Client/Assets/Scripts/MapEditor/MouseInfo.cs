using Assets.Scripts.MapEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using Utils.NET.Geometry;

public class MouseInfo : MonoBehaviour
{
    private enum Quadrant
    {
        UpperRight,
        UpperLeft,
        LowerLeft,
        LowerRight
    }

    public Camera mainCamera;

    private TextMeshProUGUI label;

    public Transform selection;

    public MapEditor editor;

    private bool dragging = false;

    private Vector3 lastDragPoint;

    private void Awake()
    {
        label = GetComponent<TextMeshProUGUI>();
    }

    private void LateUpdate()
    {
        CheckInputs();

        var mousePos = Input.mousePosition;
        var bounds = label.textBounds;

        var offset = new Vector3(Screen.width - (bounds.size.x + 20), bounds.size.y + 20, 0);

        var angleTo = new Vec2(offset.x, offset.y).AngleTo(new Vec2(mousePos.x, mousePos.y)) * Mathf.Rad2Deg;
        if (angleTo < 0)
            angleTo += 360;

        var quadrant = (Quadrant)(int)(angleTo / 90);
        transform.position = mousePos + GetCursorOffset(quadrant);

        label.alignment = GetAlignment(quadrant);

        var labelPos = mainCamera.ScreenToWorldPoint(mousePos);
        label.text = $"({(int)labelPos.x}, {(int)labelPos.y})";

        if (editor.objLayout.selectedType == MapEditorObjectType.Tile || editor.objLayout.selectedType == MapEditorObjectType.Object)
        {
            label.text += '\n';
            label.text += editor.objLayout.selectedObject.info.name;
        }

        if (editor.objLayout.selectedType == MapEditorObjectType.Tile)
        {
            label.text += '\n';
            label.text += editor.tileRotation;
        }
        else if (editor.objLayout.selectedType == MapEditorObjectType.Region)
        {
            var selected = (MapEditorRegion)editor.objLayout.selectedObject;
            label.text += '\n';
            label.text += selected.region;
        }

        if (editor.currentTool is CircleTool circleTool)
        {
            label.text += '\n';
            label.text += $"Diameter: {circleTool.diameter}";

            label.text += '\n';
            label.text += $"Mode: {(circleTool.clearMode ? "Clear" : "Draw")}";
        }

        if (editor.currentTool is FillTool fillTool)
        {
            if (fillTool.customSample)
            {
                label.text += '\n';
                label.text += $"Sample: {fillTool.sampleType}";
            }

            label.text += '\n';
            label.text += $"Mode: {(fillTool.clearMode ? "Clear" : "Draw")}";
        }

        selection.transform.localPosition = new Vector3((int)labelPos.x, (int)labelPos.y, -8);
    }

    private void CheckInputs()
    {
        var mousePos = Input.mousePosition;

        var scroll = Input.mouseScrollDelta;
        if (scroll.y != 0)
        {
            var delta = scroll.y > 0 ? 1 : -1;
            mainCamera.orthographicSize = Math.Max(Math.Min(mainCamera.orthographicSize - delta, 30), 10);
        }

        if (dragging)
        {
            mainCamera.transform.localPosition -= (mousePos - lastDragPoint) / (Screen.height / (mainCamera.orthographicSize * 2));
            lastDragPoint = mousePos;
        }

        if (Input.GetMouseButtonDown(1))
        {
            dragging = true;
            lastDragPoint = mousePos;
        }

        if (Input.GetMouseButtonUp(1))
        {
            dragging = false;
        }
    }

    private Vector3 GetCursorOffset(Quadrant quadrant)
    {
        switch (quadrant)
        {
            case Quadrant.UpperRight:
                return new Vector3(-4, -20, 0);
            case Quadrant.UpperLeft:
                return new Vector3(12, -20, 0);
            case Quadrant.LowerLeft:
                return new Vector3(12, 0, 0);
            case Quadrant.LowerRight:
                return new Vector3(-4, 0, 0);
        }
        return Vector3.zero;
    }

    private TextAlignmentOptions GetAlignment(Quadrant quadrant)
    {
        switch (quadrant)
        {
            case Quadrant.UpperRight:
                return TextAlignmentOptions.TopRight;
            case Quadrant.UpperLeft:
                return TextAlignmentOptions.TopLeft;
            case Quadrant.LowerLeft:
                return TextAlignmentOptions.BottomLeft;
            case Quadrant.LowerRight:
                return TextAlignmentOptions.BottomRight;
        }
        return TextAlignmentOptions.BottomRight;
    }
}