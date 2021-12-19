using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Data;
using TitanCore.Data.Map;
using UnityEngine;
using Utils.NET.Geometry;

public class ContextualWall : Wall
{
    public override GameObjectType ObjectType => GameObjectType.ContextualWall;

    private enum VisionMeshType
    {
        Side,
        Corner,
        Side2,
        Side3,
        Side4
    }

    private enum VisionType
    {
        None = 0,

        Down = 1,
        Right = 2,
        Up = 4,
        Left = 8,

        DownRight = Down | Right,
        RightUp = Right | Up,
        UpLeft = Up | Left,
        LeftDown = Left | Down,

        DownUp = Down | Up,
        RightLeft = Right | Left,

        DownRightUp = Down | Right | Up,
        RightUpLeft = Right | Up | Left,
        UpLeftDown = Up | Left | Down,
        LeftDownRight = Left | Down | Right,

        DownRightUpLeft = Down | Right | Up | Left
    }

    private static VisionMeshType[] visionTypes = (VisionMeshType[])Enum.GetValues(typeof(VisionMeshType));

    private MeshGroup[] meshes;

    private int vision;

    public override void LoadObjectInfo(GameObjectInfo info)
    {
        base.LoadObjectInfo(info);

        var contextInfo = (ContextualWallInfo)info;

        var meshName = contextInfo.meshNames[UnityEngine.Random.Range(0, contextInfo.meshNames.Length)];

        meshes = new MeshGroup[visionTypes.Length];
        for (int i = 0; i < visionTypes.Length; i++)
        {
            meshes[i] = MeshManager.GetMesh($"{meshName}-{visionTypes[i].ToString().ToLower()}");
        }
    }

    public override void UpdateVision(MeshFace face, bool visible)
    {
        if (face == MeshFace.Top) return;

        int faceValue = GetFaceValue(face);
        if (visible)
            vision |= faceValue;
        else
            vision &= ~faceValue;

        meshFilter.mesh = GetMesh();
        transform.localEulerAngles = new Vector3(0, 0, GetRotation());
    }

    private int GetFaceValue(MeshFace face)
    {
        switch (face)
        {
            case MeshFace.Down:
            case MeshFace.Right:
                return 1 << (int)face;
            case MeshFace.Up:
                return 1 << 2;
            case MeshFace.Left:
                return 1 << 3;
            default:
                return 0;
        }
    }

    private float GetRotation()
    {
        switch ((VisionType)vision)
        {
            case VisionType.Right:
                return 0;
            case VisionType.Up:
                return 90;
            case VisionType.Left:
                return 180;
            case VisionType.Down:
                return 270;

            case VisionType.RightUp:
                return 0;
            case VisionType.UpLeft:
                return 90;
            case VisionType.LeftDown:
                return 180;
            case VisionType.DownRight:
                return 270;

            case VisionType.RightLeft:
                return 0;
            case VisionType.DownUp:
                return 180;

            case VisionType.RightUpLeft:
                return 180;
            case VisionType.UpLeftDown:
                return 270;
            case VisionType.LeftDownRight:
                return 0;
            case VisionType.DownRightUp:
                return 90;

            case VisionType.DownRightUpLeft:
                return 0;
        }
        return 0;
    }

    private Mesh GetMesh()
    {
        switch ((VisionType)vision)
        {
            case VisionType.Right:
            case VisionType.Up:
            case VisionType.Left:
            case VisionType.Down:
                return meshes[(int)VisionMeshType.Side].mesh;

            case VisionType.RightUp:
            case VisionType.UpLeft:
            case VisionType.LeftDown:
            case VisionType.DownRight:
                return meshes[(int)VisionMeshType.Corner].mesh;

            case VisionType.RightLeft:
            case VisionType.DownUp:
                return meshes[(int)VisionMeshType.Side2].mesh;

            case VisionType.RightUpLeft:
            case VisionType.UpLeftDown:
            case VisionType.LeftDownRight:
            case VisionType.DownRightUp:
                return meshes[(int)VisionMeshType.Side3].mesh;

            case VisionType.DownRightUpLeft:
                return meshes[(int)VisionMeshType.Side4].mesh;
        }
        return null;
    }

    public override void SetPosition(Vec2 position, bool first)
    {
        transform.localPosition = position.ToVector2() + new Vector2(0.5f, 0.5f);
    }
}
