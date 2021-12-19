using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Mobile
{
    public class NearbyPlayerMobile : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
    {
        public Image characterSprite;

        public TextMeshProUGUI nameLabel;

        public ItemDisplay[] equips;

        public RectTransform healthBarRect;

        public TooltipManager tooltipManager;

        public ActionsPopup popupActions;

        private Character character;

        private bool isDown = false;

        private int downId;

        private float downTime;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (character == null) return;
            tooltipManager.ShowTooltip(character);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            isDown = true;
            downId = eventData.pointerId;
            downTime = 0;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isDown = false;
        }

        public void SetCharacter(Character character)
        {
            this.character = character;

            gameObject.SetActive(character != null);
            if (!gameObject.activeSelf) return;

            var info = (TitanCore.Data.Entities.CharacterInfo)character.info;

            characterSprite.sprite = TextureManager.GetDisplaySprite(character.GetSkinInfo());

            for (int i = 0; i < equips.Length && i < info.equipSlots.Length; i++)
                equips[i].SetPlaceholderType(info.equipSlots[i]);

            for (int i = 0; i < equips.Length; i++)
                equips[i].SetItem(character.GetItem(i));

            nameLabel.text = $"{character.playerName} | Lvl {character.GetLevel()}";

            healthBarRect.anchorMax = new Vector2(Mathf.Max(0, character.serverHealth / (float)character.GetStatFunctional(StatType.MaxHealth)), 1);
            healthBarRect.offsetMax = Vector2.zero;
        }

        private void LateUpdate()
        {
            if (isDown)
            {
                downTime += Time.deltaTime;
                if (downTime > 0.6f)
                {
                    isDown = false;
                    //ShowActions();
                }
            }

            if (character == null) return;

            healthBarRect.anchorMax = new Vector2(Mathf.Max(0, character.serverHealth / (float)character.GetStatFunctional(StatType.MaxHealth)), 1);
            healthBarRect.offsetMax = Vector2.zero;
        }

        private void ShowActions()
        {
#if UNITY_EDITOR
            popupActions.Show(character, Input.mousePosition);
#else
            Touch touch = default;
            for (int i = 0; i < Input.touchCount; i++)
            {
                var t = Input.GetTouch(i);
                if (t.fingerId == downId)
                {
                    touch = t;
                    break;
                }
            }

            popupActions.Show(character, touch.position);
#endif
        }
    }
}
