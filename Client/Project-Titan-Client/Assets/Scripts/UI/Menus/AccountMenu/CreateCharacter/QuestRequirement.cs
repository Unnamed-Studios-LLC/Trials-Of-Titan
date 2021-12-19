using System.Collections;
using System.Collections.Generic;
using TitanCore.Core;
using TitanCore.Data.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils.NET.Utils;

public class QuestRequirement : MonoBehaviour
{

    public Image marker;

    public TextMeshProUGUI label;

    public Color completedColor = Color.white;

    public Color pendingColor = Color.white;

    public void SetRequirement(ClassRequirement requirement)
    {
        label.text = $"{requirement.questRequirement} {requirement.classType} {StringUtils.ApplyPlural("Quest", requirement.questRequirement)}";

        var quest = Account.GetClassQuest((ushort)requirement.classType);
        if (quest.GetCompletedCount() >= requirement.questRequirement)
            marker.color = completedColor;
        else
            marker.color = pendingColor;
    }
}
