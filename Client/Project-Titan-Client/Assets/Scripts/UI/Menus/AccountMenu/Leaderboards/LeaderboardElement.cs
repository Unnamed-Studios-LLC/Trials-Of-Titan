using System.Collections;
using System.Collections.Generic;
using TitanCore.Data;
using TitanCore.Net.Web;
using TMPro;
using UnityEngine;

public class LeaderboardElement : MonoBehaviour
{
    public TextMeshProUGUI nameLabel;

    public ClassPreview classPreview;

    public ItemDisplay[] equips;

    public TextMeshProUGUI valueLabel;

    public bool living = false;

    public virtual void SetInfo(int index, WebLeaderboardInfo info)
    {
        nameLabel.text = $"{index + 1}. {info.name}";

        classPreview.SetClass(GameData.objects[info.skin != 0 ? info.skin : info.type]);

        var classInfo = (TitanCore.Data.Entities.CharacterInfo)GameData.objects[info.type];

        for (int i = 0; i < classInfo.equipSlots.Length; i++)
            equips[i].SetPlaceholderType(classInfo.equipSlots[i]);

        for (int i = 0; i < equips.Length && i < info.equips.Length; i++)
            equips[i].SetItem(info.equips[i]);

        if (living)
        {
            valueLabel.text = $"{Constants.Souls_Sprite}{info.value / 1000}";
        }
        else
        {
            valueLabel.text = $"{Constants.Death_Currency_Sprite}{info.value}";
        }
    }
}
