using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Core;
using TitanCore.Data.Components;
using TitanCore.Data.Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pc
{
    public class MarketItemTooltip : PcTooltip<MarketDisplay>, IDescriber
    {
        private const string lineText = "<line-height=80%><size=16.67%><sprite=\"LabelSprites\" name=\"Line\" color=#000000>\n</size></line-height>";

        private const string Click_Sprite = "<size=150%><sprite=\"LabelSprites\" name=\"LeftClick\"></size>";

        public override bool AutoSize => true;

        public override bool FollowMouse => false;

        public Image itemImage;

        public TextMeshProUGUI nameLabel;

        public TextMeshProUGUI contentLabel;

        public TextMeshProUGUI tierLabel;

        public Color cantUseColor;

        public Color soulboundColor = Color.white;

        public Color neutralColor = new Color(233 / 255.0f, 187 / 255.0f, 48 / 255.0f, 1);

        private static Color greenColor = new Color(0.2938324f, 0.8773585f, 0.3517506f, 1);

        private static Color redColor = new Color(0.8773585f, 0.2300355f, 0.2027857f, 1);

        private bool justAddedLine = false;

        protected override void Load(Player player, bool owned, MarketDisplay obj)
        {
            var item = obj.purchasableItem;
            var info = item.GetInfo();
            var equip = info as EquipmentInfo;
            var weapon = info as WeaponInfo;

            itemImage.sprite = TextureManager.GetSprite(info.textures[0].displaySprite);
            nameLabel.text = info.name;

            itemImage.material = MaterialManager.GetUIMaterial(item.enchantLevel);

            if (equip != null)
            {
                tierLabel.text = equip.GetTierDisplay();
                tierLabel.color = ItemDisplay.GetTierColor(equip.tier);
            }
            else
                tierLabel.text = "";

            var myClass = (TitanCore.Data.Entities.CharacterInfo)player.info;
            ItemDescriber.Describe(this, myClass, owned, item, player.GetStatFunctional(StatType.Attack));

            if (item.soulbound)
            {
                SetBackgroundColor(soulboundColor);
            }
            else if (equip != null && !myClass.CanUseSloType(info.slotType))
            {
                SetBackgroundColor(cantUseColor);
            }

            contentLabel.ForceMeshUpdate();
            contentLabel.rectTransform.sizeDelta = contentLabel.bounds.size;

            background.rectTransform.anchoredPosition = new Vector2(-Screen.height * 0.26f, 0);
        }

        public void NewLine()
        {
            contentLabel.text += '\n';
        }

        public void AddContent(string content)
        {
            contentLabel.text += content;
            justAddedLine = false;
        }

        public void AddContent(string content, GameColor color)
        {
            AddContent($"<color=#{ColorUtility.ToHtmlStringRGB(color.ToUnityColor())}>{content}</color>");
            justAddedLine = false;
        }

        public void AddTitle(string name)
        {
            contentLabel.text += name + ':';
            justAddedLine = false;
        }

        public void AddTitle(string name, GameColor color)
        {
            AddContent($"<color=#{ColorUtility.ToHtmlStringRGB(color.ToUnityColor())}>{name}</color>");
        }

        public void AddElement(string element)
        {
            contentLabel.text += ' ' + element;
            justAddedLine = false;
        }

        public void AddElement(string element, GameColor color)
        {
            AddElement($"<color=#{ColorUtility.ToHtmlStringRGB(color.ToUnityColor())}>{element}</color>");
        }

        public void AddLineSeparator()
        {
            if (justAddedLine) return;
            contentLabel.text += lineText;
            justAddedLine = true;
        }

        public GameColor GetNeutralColor()
        {
            return neutralColor.ToGameColor();
        }

        public GameColor GetEnchantColor(int enchantLevel)
        {
            var material = MaterialManager.GetUIMaterial(enchantLevel);
            return material.GetColor("_GlowColor").ToGameColor();
        }

        public void Clear()
        {
            contentLabel.text = "";
        }

        public bool GetAllowsEmptyTitle()
        {
            return true;
        }

        public bool GetAllowsSprites()
        {
            return true;
        }

        public string GetClickSprite()
        {
            return Click_Sprite;
        }
    }
}
