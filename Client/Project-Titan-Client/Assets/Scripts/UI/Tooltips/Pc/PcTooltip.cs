using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Pc
{
    public abstract class PcTooltip<T> : Tooltip
    {
        public Image background;

        public Image outline;

        public RectTransform content;

        protected Vector2 contentSize;
        protected Vector2 ContentSize
        {
            get => contentSize;
            set
            {
                background.rectTransform.sizeDelta = new Vector2(value.x + 10, value.y + 10);
                contentSize = value;
            }
        }

        public abstract bool AutoSize { get; }

        public virtual bool FollowMouse => true;

        protected virtual void Awake()
        {
            ContentSize = background.rectTransform.sizeDelta - new Vector2(10, 10);
        }

        public override void LoadObject(Player player, bool owned, object obj)
        {
            Load(player, owned, (T)obj);
            if (AutoSize)
                ResizeContent();
        }

        protected abstract void Load(Player player, bool owned, T obj);

        public override void Hide()
        {
            Destroy(gameObject);
        }


        protected void ResizeContent()
        {
            var children = content.GetComponentsInChildren<RectTransform>();
            var max = new Vector2(0, 0);
            foreach (var child in children)
            {
                if (child == content) continue;
                var pos = child.position;
                var size = child.sizeDelta;
                var offset = size * child.pivot;
                pos -= new Vector3(offset.x, offset.y, 0);
                pos -= content.position;

                max.x = Mathf.Max(max.x, pos.x + size.x);
                max.y = Mathf.Max(max.y, -pos.y);
            }
            ContentSize = max;
        }

        protected void SetBackgroundColor(Color color)
        {
            background.color = color;
        }

        protected void SetOutlineColor(Color color)
        {
            outline.color = color;
        }

        protected virtual void LateUpdate()
        {
            if (!FollowMouse) return;

            var mousePos = Input.mousePosition;
            mousePos = new Vector3((int)mousePos.x, (int)mousePos.y, mousePos.z);

            var size = background.rectTransform.rect.size + new Vector2(16, 16);
            var offset = Vector3.zero;

            if (mousePos.x - size.x < 0)
                offset.x += size.x + 16;
            if (mousePos.y + size.y > Screen.height)
                offset.y += -size.y - 16;

            background.rectTransform.anchoredPosition = mousePos + new Vector3(-16, 16) + offset;
        }
    }
}
