using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pc
{
    public class PlayerTooltip : PcTooltip<Character>
    {
        public override bool AutoSize => false;

        public Image characterSprite;

        public TextMeshProUGUI nameLabel;

        public RectTransform healthRect;

        public ItemDisplay[] equips;

        protected override void Load(Player player, bool owned, Character obj)
        {
            var info = (TitanCore.Data.Entities.CharacterInfo)obj.info;

            characterSprite.sprite = TextureManager.GetDisplaySprite(info);
            nameLabel.text = obj.playerName + $" <size=12>(Level {obj.GetLevel()})</size>";

            for (int i = 0; i < 4; i++)
            {
                var equip = equips[i];
                equip.SetPlaceholderType(info.equipSlots[i]);
                equip.SetItem(obj.GetItem(i));
            }

            healthRect.anchorMax = new Vector2(Mathf.Max(0, obj.serverHealth / (float)obj.GetStatFunctional(StatType.MaxHealth)), 1);
            healthRect.offsetMax = Vector2.zero;
        }
    }
}
