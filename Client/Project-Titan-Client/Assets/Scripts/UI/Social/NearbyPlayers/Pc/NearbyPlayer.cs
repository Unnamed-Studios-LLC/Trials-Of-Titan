using System.Collections;
using System.Collections.Generic;
using TitanCore.Data.Entities;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Pc
{
    public class NearbyPlayer : UnityEngine.MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public Image spriteImage;

        public TextMeshProUGUI nameLabel;

        private Character character;

        public TooltipManager tooltipManager;

        public ActionsPopup actions;

        private int tooltipId = -1;

        public void OnPointerEnter(PointerEventData eventData)
        {
            ShowTooltip();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            HideTooltip();
        }

        private void OnDisable()
        {
            HideTooltip();
        }

        private void ShowTooltip()
        {
            if (character == null || character.world == null) return;
            tooltipId = tooltipManager.ShowTooltip(character);
        }

        private void HideTooltip()
        {
            if (tooltipId == -1) return;
            tooltipManager.HideTooltip(tooltipId);
            tooltipId = -1;
        }

        public void SetCharacter(Character character)
        {
            this.character = character;
            gameObject.SetActive(character != null);
            if (character == null) return;

            //var classInfo = (CharacterInfo)character.info;

            var skinInfo = character.GetSkinInfo();

            var sprite = TextureManager.GetDisplaySprite(skinInfo);

            float aspect = sprite.textureRect.width / sprite.textureRect.height;

            var rect = spriteImage.rectTransform.rect;
            var size = spriteImage.rectTransform.sizeDelta;
            spriteImage.rectTransform.sizeDelta = new UnityEngine.Vector2(rect.height * aspect, size.y);
            spriteImage.sprite = sprite;

            nameLabel.text = character.playerName;

            nameLabel.rectTransform.offsetMin = new UnityEngine.Vector2(spriteImage.rectTransform.sizeDelta.x, 0);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            HideTooltip();
            actions.Show(character, Input.mousePosition);
        }
    }
}