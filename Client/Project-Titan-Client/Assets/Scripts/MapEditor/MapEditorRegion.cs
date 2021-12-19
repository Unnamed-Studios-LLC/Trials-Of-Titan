using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Core;
using TitanCore.Data;
using UnityEngine;

namespace Assets.Scripts.MapEditor
{
    public class MapEditorRegion : MapEditorObject
    {
        public SpriteRenderer spriteRenderer;

        public Region region;

        public override ushort Id => (ushort)region;

        public void SetRegion(Region region)
        {
            this.region = region;
            var color = RegionColors.Get(region);
            color.a = 0.5f;
            spriteRenderer.color = color;
        }

        public override void SetPosition(Vector3 position)
        {
            var sprite = spriteRenderer.sprite;
            var width = sprite.rect.width;
            var height = sprite.rect.height;

            Vector2 offset;
            if (width > height)
            {
                offset = new Vector2(0, (-1 + height / width) / 2);
            }
            else
            {
                offset = new Vector2((1 - width / height) / 2, 0);
            }

            position += new Vector3(offset.x, offset.y, 0);

            base.SetPosition(position);
        }
    }
}
