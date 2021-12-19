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
    public class MapEditorSprite : MapEditorObject
    {
        public SpriteRenderer spriteRenderer;

        public override void LoadInfo(GameObjectInfo info, bool forDisplay)
        {
            base.LoadInfo(info, forDisplay);

            string spriteName = info.textures[0].displaySprite;
            if (!forDisplay)
            {
                if (info is TileInfo)
                {
                    spriteName = info.textures[UnityEngine.Random.Range(0, info.textures.Length)].displaySprite;
                }
            }

            var sourceSprite = TextureManager.GetSprite(spriteName);

            if (sourceSprite != null)
            {
                var largeSide = Math.Max(sourceSprite.rect.width, sourceSprite.rect.height);
                var sprite = Sprite.Create(sourceSprite.texture, sourceSprite.textureRect, new Vector2(0, 1), largeSide, 0, SpriteMeshType.FullRect, Vector4.zero, false);
                spriteRenderer.sprite = sprite;
            }
        }

        public override void SetPosition(Vector3 position)
        {
            var sprite = spriteRenderer.sprite;
            if (sprite != null)
            {
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
            }

            base.SetPosition(position);
        }
    }
}
