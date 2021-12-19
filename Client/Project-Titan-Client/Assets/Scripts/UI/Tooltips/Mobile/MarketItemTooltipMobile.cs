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

namespace Mobile
{
    public class MarketItemTooltipMobile : MobileTooltip<MarketDisplay>, IDescriber
    {
        private const string Click_Sprite = "<line-height=150%><sprite=\"LabelSprites\" name=\"LeftClick\"></line-height>";

        public ItemDisplay item;

        public TextMeshProUGUI nameLabel;

        public TextMeshProUGUI contentLabel;

        public RectTransform button0;

        public TextMeshProUGUI button0Label;

        public RectTransform button1;

        public TextMeshProUGUI button1Label;

        private MarketDisplay display;

        private static Color neutralColor = new Color(244.0f / 255.0f, 255.0f / 255.0f, 144.0f / 255.0f, 1);

        protected override void Load(Player player, bool owned, MarketDisplay obj)
        {
            display = obj;

            var info = obj.purchasableItem.GetInfo();
            var equip = info as EquipmentInfo;
            var weapon = info as WeaponInfo;

            item.SetItem(obj.purchasableItem);
            nameLabel.text = info.name;

            if (obj.deathCurrenyCost != 0 && obj.premiumCurrenyCost != 0)
            {
                button1.gameObject.SetActive(true);
                button0Label.text = $"{Constants.Premium_Currency_Sprite}{obj.premiumCurrenyCost}";
                button1Label.text = $"{Constants.Death_Currency_Sprite}{obj.deathCurrenyCost}";
            }
            else if (obj.premiumCurrenyCost != 0)
            {
                button1.gameObject.SetActive(false);
                button0Label.text = $"{Constants.Premium_Currency_Sprite}{obj.premiumCurrenyCost}";
            }
            else if (obj.deathCurrenyCost != 0)
            {
                button1.gameObject.SetActive(false);
                button0Label.text = $"{Constants.Death_Currency_Sprite}{obj.deathCurrenyCost}";
            }

            /*
            if (equip != null)
            {
                tierLabel.text = equip.GetTierDisplay();
                tierLabel.color = ItemDisplay.GetTierColor(equip.tier);
            }
            else
                tierLabel.text = "";
            */

            ItemDescriber.Describe(this, (TitanCore.Data.Entities.CharacterInfo)player.info, owned, obj.purchasableItem, player.GetStatFunctional(StatType.Attack));
        }


        public void NewLine()
        {
            contentLabel.text += '\n';
        }

        public void AddContent(string content)
        {
            contentLabel.text += content;
        }

        public void AddContent(string content, GameColor color)
        {
            AddContent($"<color=#{ColorUtility.ToHtmlStringRGB(color.ToUnityColor())}>{content}</color>");
        }

        public void AddTitle(string name)
        {
            contentLabel.text += name + ':';
        }

        public void AddTitle(string name, GameColor color)
        {
            AddContent($"<color=#{ColorUtility.ToHtmlStringRGB(color.ToUnityColor())}>{name}</color>");
        }

        public void AddElement(string element)
        {
            contentLabel.text += ' ' + element;
        }

        public void AddElement(string element, GameColor color)
        {
            AddElement($"<color=#{ColorUtility.ToHtmlStringRGB(color.ToUnityColor())}>{element}</color>");
        }

        public void AddLineSeparator()
        {
            contentLabel.text += "\n";
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

        public void OnButton0()
        {
            display.Interact(0);
        }

        public void OnButton1()
        {
            display.Interact(1);
        }
    }
}
