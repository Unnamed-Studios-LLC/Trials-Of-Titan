using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Mobile
{
    public class Joystick : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public RectTransform anchor;

        public RectTransform bar;

        public RectTransform thumbstick;

        public bool didActivate = false;

        public bool active = false;

        public float angle = 0;

        public float distanceScalar;

        private void Awake()
        {
            Hide();
            ScaleElements();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            anchor.gameObject.SetActive(true);
            thumbstick.gameObject.SetActive(true);

            anchor.position = eventData.position;
            thumbstick.position = eventData.position;
        }

        public void OnDrag(PointerEventData eventData)
        {
            thumbstick.position = eventData.position;

            float distance = Vector2.Distance(thumbstick.position, anchor.position);
            float barDistance = GetBarHideDistance();

            var thumbstickVector = thumbstick.position - anchor.position;
            angle = Mathf.Atan2(thumbstickVector.y, thumbstickVector.x) * Mathf.Rad2Deg;

            bar.gameObject.SetActive(distance >= barDistance);
            if (bar.gameObject.activeSelf)
            {
                var size = bar.sizeDelta;
                size.x = distance - barDistance;
                bar.sizeDelta = size;

                bar.position = anchor.position + thumbstickVector * 0.5f;
                bar.localEulerAngles = new Vector3(0, 0, angle);
            }

            float activeDistance = thumbstick.sizeDelta.x * 0.4f;
            bool wasActive = active;
            active = distance > activeDistance;
            didActivate = !wasActive && active;

            var minScreen = Mathf.Min(Screen.width / 2, Screen.height);
            var maxScalarDistance = minScreen * 0.4f;
            distanceScalar = Mathf.Max(0, Mathf.Min(1, (distance - activeDistance) / maxScalarDistance));
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            active = false;

            Hide();
        }

        private void Hide()
        {
            anchor.gameObject.SetActive(false);
            bar.gameObject.SetActive(false);
            thumbstick.gameObject.SetActive(false);
        }

        private void ScaleElements()
        {
            var size = Screen.height * 0.11f;
            anchor.sizeDelta = new Vector2(size, size);
            thumbstick.sizeDelta = new Vector2(size, size);
            bar.sizeDelta = new Vector2(0, size * 0.1f);
        }

        private float GetBarHideDistance()
        {
            return anchor.sizeDelta.x * 1.1f;
        }
    }
}
