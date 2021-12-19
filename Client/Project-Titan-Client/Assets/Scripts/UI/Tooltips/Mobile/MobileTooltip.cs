using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Mobile
{
    public abstract class MobileTooltip<T> : Tooltip
    {
        private const float Tooltip_Animation_Time = 0.2f;

        private LTDescr tween;

        public override void LoadObject(Player player, bool owned, object obj)
        {
            Load(player, owned, (T)obj);

            var rectTransform = GetComponent<RectTransform>();
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            var rect = rectTransform.rect;
            rectTransform.anchoredPosition = new Vector2(-rect.width, 0);

            tween = LeanTween.value(gameObject, (val) =>
            {
                rectTransform.anchoredPosition = val;
            }, rectTransform.anchoredPosition, new Vector2(0, rectTransform.anchoredPosition.y), Tooltip_Animation_Time).setEaseInSine().setOnComplete(() => tween = null);
        }

        protected abstract void Load(Player player, bool owned, T obj);

        public override void Hide()
        {
            LeanTween.cancel(gameObject);

            var rectTransform = GetComponent<RectTransform>();
            var rect = rectTransform.rect;

            LeanTween.value(gameObject, (val) =>
            {
                rectTransform.anchoredPosition = val;
            }, rectTransform.anchoredPosition, new Vector2(-rect.width, rectTransform.anchoredPosition.y), Tooltip_Animation_Time).setEaseInSine().setDestroyOnComplete(true);
        }
    }
}
