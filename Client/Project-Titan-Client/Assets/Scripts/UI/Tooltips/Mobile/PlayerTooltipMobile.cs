using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Mobile
{
    public class PlayerTooltipMobile : MobileTooltip<Character>
    {
        public Image characterSprite;

        public TextMeshProUGUI nameLabel;

        public TextMeshProUGUI levelLabel;

        public RectTransform healthBar;

        public TextMeshProUGUI healthText;

        public ItemDisplay[] equips;

        private Character character;

        private World world;

        protected override void Load(Player player, bool owned, Character obj)
        {
            world = player.world;
            character = obj;

            characterSprite.sprite = TextureManager.GetDisplaySprite(character.GetSkinInfo());

            nameLabel.text = obj.playerName;
            levelLabel.text = "Level " + obj.GetLevel();

            for (int i = 0; i < equips.Length; i++)
                equips[i].SetPlaceholderType(character.GetSlotType(i));

            UpdateOthers();
        }

        private void LateUpdate()
        {
            if (character == null) return;
            UpdateOthers();
        }

        private void UpdateOthers()
        {
            for (int i = 0; i < equips.Length; i++)
                equips[i].SetItem(character.GetItem(i));

            healthBar.anchorMax = new Vector2(Mathf.Max(0, character.serverHealth / (float)character.GetStatFunctional(StatType.MaxHealth)), 1);
            healthBar.offsetMax = Vector2.zero;

            healthText.text = character.serverHealth.ToString();
        }

        public void Teleport()
        {
            world.Teleport(character);
            CloseTooltip();

            var mobileUI = (MobileGameUI)world.gameManager.ui;
            mobileUI.sideMenuManager.CloseMenu();
        }

        public void Trade()
        {
            world.Trade(character);
            CloseTooltip();
        }

        public void Message()
        {

            CloseTooltip();
        }
    }
}
