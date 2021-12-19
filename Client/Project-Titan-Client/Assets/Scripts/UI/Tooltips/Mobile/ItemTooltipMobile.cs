using Pc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Core;
using TitanCore.Data;
using TitanCore.Data.Components;
using TitanCore.Data.Items;
using TitanCore.Net;
using TMPro;
using UnityEngine;
using Utils.NET.Utils;

namespace Mobile
{
    public class ItemTooltipMobile : MobileTooltip<Item>, IDescriber
    {
        private const string Click_Sprite = "<line-height=150%><sprite=\"LabelSprites\" name=\"LeftClick\"></line-height>";

        public ItemDisplay item;

        public TextMeshProUGUI nameLabel;

        public TextMeshProUGUI contentLabel;

        private static Color neutralColor = new Color(244.0f / 255.0f, 255.0f / 255.0f, 144.0f / 255.0f, 1);

        private static Color redColor = new Color(0.8773585f, 0.2300355f, 0.2027857f, 1);

        protected override void Load(Player player, bool owned, Item obj)
        {
            var info = obj.GetInfo();
            var equip = info as EquipmentInfo;
            var weapon = info as WeaponInfo;

            item.SetItem(obj);
            nameLabel.text = info.name;

            /*
            if (equip != null)
            {
                tierLabel.text = equip.GetTierDisplay();
                tierLabel.color = ItemDisplay.GetTierColor(equip.tier);
            }
            else
                tierLabel.text = "";
            */

            var myClass = (TitanCore.Data.Entities.CharacterInfo)player.info;
            ItemDescriber.Describe(this, myClass, owned, obj, player.GetStatFunctional(StatType.Attack));
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
    }
}
