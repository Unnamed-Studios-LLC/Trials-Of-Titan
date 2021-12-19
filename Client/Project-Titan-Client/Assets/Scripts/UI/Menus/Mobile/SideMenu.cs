using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Mobile
{
    public class SideMenu : MonoBehaviour
    {
        private const float Button_Animation_Time = 0.15f;

        protected const float Menu_Animation_Time = 0.15f;

        public RectTransform rectTransform;

        public RectTransform button;

        public float buttonScaling = 1;

        public bool closeOnDamage = false;

        private float buttonOffset = 0;

        public void SizeButton(int index)
        {
            var buttonSize = Screen.height * 0.08f;
            button.sizeDelta = new Vector2(buttonSize * (button.sizeDelta.x / button.sizeDelta.y), buttonSize);
            button.anchoredPosition = new Vector2(0, -buttonSize / 2 - buttonSize * index * 1.2f);
        }

        protected Vector2 GetHiddenPosition()
        {
            return new Vector2(rectTransform.rect.size.x + 1, 0);
        }

        private Vector2 GetButtonSize()
        {
            return button.sizeDelta;
        }

        public void ReturnButton()
        {
            if (button == null) return;
            LeanTween.scale(button.gameObject, new Vector3(1, 1, 1), Button_Animation_Time);
            LeanTween.value(button.gameObject, (val) =>
            {
                button.anchoredPosition = val;
            }, button.anchoredPosition, new Vector2(-GetButtonSize().x / 2 + buttonOffset, button.anchoredPosition.y), Button_Animation_Time).setEaseInSine();
        }

        public void SetButtonOffset(float buttonOffset)
        {
            this.buttonOffset = buttonOffset;
        }

        public void HideButton()
        {
            if (button == null) return;
            LeanTween.value(button.gameObject, (val) =>
            {
                button.anchoredPosition = val;
            }, button.anchoredPosition, new Vector2(GetButtonSize().x, button.anchoredPosition.y), Button_Animation_Time).setEaseInSine();
        }

        public void ExpandButton()
        {
            if (button == null) return;
            float xPosition = -GetHiddenPosition().x - button.sizeDelta.y / 2 + buttonOffset;
            float minX = -Screen.width + button.sizeDelta.x * 0.5f + button.sizeDelta.x * buttonScaling;

            xPosition = Mathf.Max(xPosition, minX);

            LeanTween.scale(button.gameObject, new Vector3(buttonScaling, buttonScaling, buttonScaling), Button_Animation_Time);
            LeanTween.value(button.gameObject, (val) =>
            {
                button.anchoredPosition = val;
            }, button.anchoredPosition, new Vector2(xPosition, button.anchoredPosition.y), Button_Animation_Time).setEaseInSine();
        }

        public void TweenToHide(LTSeq seq)
        {
            ReturnButton();
            seq.append(rectTransform.LeanMove((Vector3)GetHiddenPosition(), Menu_Animation_Time).setEaseInSine());
            seq.append(() => gameObject.SetActive(false));
        }

        public void TweenToShow(LTSeq seq)
        {
            ExpandButton();
            seq.append(() => gameObject.SetActive(true));
            seq.append(rectTransform.LeanMove(new Vector3(0, 0, 0), Menu_Animation_Time).setEaseOutSine());/*LeanTween.value(gameObject, (val) =>
            {
                rectTransform.anchoredPosition = val;
            }, rectTransform.anchoredPosition, new Vector2(0, 0), Menu_Animation_Time).setEaseOutSine());*/
        }

        public void SetHidden()
        {
            rectTransform.anchoredPosition = GetHiddenPosition();
            gameObject.SetActive(false);
            button.anchoredPosition = new Vector2(-GetButtonSize().x / 2, button.anchoredPosition.y);
        }
    }
}
