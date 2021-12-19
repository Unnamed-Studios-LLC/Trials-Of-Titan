using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Data;
using TitanCore.Data.Map;
using UnityEngine;

namespace Assets.Scripts.MapEditor
{
    public class MapEditorMesh : MapEditorObject
    {
        public MeshFilter meshFilter;

        public override void LoadInfo(GameObjectInfo info, bool forDisplay)
        {
            base.LoadInfo(info, forDisplay);

            var obj3dInfo = (Object3dInfo)info;

            string meshName;
            if (forDisplay)
                meshName = obj3dInfo.meshNames[0];
            else
                meshName = obj3dInfo.meshNames[UnityEngine.Random.Range(0, obj3dInfo.meshNames.Length)];
            var meshGroup = MeshManager.GetMesh(meshName);
            meshFilter.mesh = meshGroup.mesh;

            float maxValue = 0;
            foreach (var vertex in meshFilter.mesh.vertices)
            {
                maxValue = Math.Max(Math.Max(maxValue, vertex.x), vertex.y);
            }
            var scale = 0.5f / maxValue;
            meshFilter.transform.localScale = new Vector3(scale, scale, scale);
        }

        public override void SetPosition(Vector3 position)
        {
            position.y -= 0.85f;
            position.x += 0.5f;

            base.SetPosition(position);
        }
    }
}
