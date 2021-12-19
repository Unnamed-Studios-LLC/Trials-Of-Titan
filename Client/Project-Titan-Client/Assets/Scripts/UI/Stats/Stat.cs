using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Core;
using TitanCore.Net;
using TMPro;
using UnityEngine;

public class Stat : MonoBehaviour
{
    protected static Color statColor = new Color(217f / 255f, 214f / 255f, 224f / 255f, 1);
    protected static Color maxStatColor = new Color(219f / 255f, 211f / 255f, 61f / 255f, 1);
    protected static Color maxAscendColor = new Color(172f / 255f, 83f / 255f, 233f / 255f, 1);

    public TextMeshProUGUI stat;

    public StatType statType;

    public virtual void SetStat(int value, int extra, Player player)
    {
        if (extra == 0)
            stat.text = (value).ToString();
        else
            stat.text = $"{value + extra} <size=60%>{(extra > 0 ? "+" : "")}{extra}</size>";

        CheckMax(value, player);
    }

    protected void CheckMax(int value, Player player)
    {
        var level = player.GetLevel();
        var data = ((TitanCore.Data.Entities.CharacterInfo)player.info).stats[statType];
        var locked = player.GetStatLock(statType);

        if (locked != 0)
        {
            if ((statType == StatType.MaxHealth && value - locked >= NetConstants.Max_Ascension * 10) || (statType != StatType.MaxHealth && value - locked >= NetConstants.Max_Ascension))
                stat.color = maxAscendColor;
            else if (value > locked)
                stat.color = World.soulColor;
            else
                stat.color = maxStatColor;
        }
        else if (value == data.maxValue)
            stat.color = maxStatColor;
        else
            stat.color = statColor;
    }
}
