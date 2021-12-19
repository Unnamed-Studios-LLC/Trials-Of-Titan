using UnityEngine;
using System.Collections;
using TitanCore.Data;
using TitanCore.Data.Map;

public class Wall : MeshWorldObject
{
    public override GameObjectType ObjectType => GameObjectType.Wall;

    public MeshFilter[] faceFilters;

    public MeshGroup meshGroup;

    protected override void Awake()
    {
        base.Awake();

        if (meshFilter == null)
            meshFilter = faceFilters[0];
    }

    public override void LoadObjectInfo(GameObjectInfo info)
    {
        base.LoadObjectInfo(info);

        var wallInfo = (WallInfo)info;

        meshGroup = MeshManager.GetMesh(wallInfo.meshNames[Random.Range(0, wallInfo.meshNames.Length)]);
        //faceFilters[(int)MeshFace.Top].mesh = meshGroup.faces[(int)MeshFace.Top];
        UpdateVision(MeshFace.Top, true);
    }

    public virtual void UpdateVision(MeshFace face, bool visible)
    {
        faceFilters[(int)face].mesh = visible ? meshGroup.faces[(int)face] : null;
    }

    public override void Enable()
    {
        base.Enable();

        transform.localEulerAngles = new Vector3(0, 0, 0);
    }
}
