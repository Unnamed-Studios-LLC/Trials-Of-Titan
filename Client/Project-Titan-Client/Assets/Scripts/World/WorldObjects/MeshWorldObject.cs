using UnityEngine;
using System.Collections;

public abstract class MeshWorldObject : WorldObject
{
    protected override bool IsSprite => false;

    public MeshFilter meshFilter;

    protected override void Awake()
    {
        base.Awake();

        meshFilter = GetComponent<MeshFilter>();
    }

    public override float GetHeight()
    {
        return 1;
    }
}
