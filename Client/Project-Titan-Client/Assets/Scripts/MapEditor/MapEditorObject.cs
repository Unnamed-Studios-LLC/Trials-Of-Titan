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
    public abstract class MapEditorObject : MonoBehaviour
    {
        public virtual ushort Id => info.id;

        public GameObjectInfo info;

        public virtual void LoadInfo(GameObjectInfo info, bool forDisplay)
        {
            this.info = info;
        }

        public virtual void SetPosition(Vector3 position)
        {
            transform.position = position;
        }
    }
}
