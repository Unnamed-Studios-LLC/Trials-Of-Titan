using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Net;
using TMPro;
using UnityEngine;

public class AccountMenu : MonoBehaviour
{
    public TextMeshProUGUI nameLabel;

    public TextMeshProUGUI classQuestLabel;

    public TextMeshProUGUI markerTiersLabel;

    public GameObject goal1;

    public GameObject goal2;

    public GameObject goal3;

    private void OnEnable()
    {
        var questCount = 0;
        foreach (var quest in Account.describe.classQuests)
        {
            for (int i = 0; i < 4; i++)
                if (quest.HasCompletedQuest(i))
                    questCount++;
        }

        nameLabel.text = $"{Constants.GetClassQuestString(questCount)}{Account.describe.name}";
        classQuestLabel.text = questCount.ToString();

        var markerBuilder = new StringBuilder();
        string lastQuest = Constants.GetClassQuestString(0);
        for (int i = 1; i <= Constants.maxQuests; i++)
        {
            var questString = Constants.GetClassQuestString(i);
            if (questString == lastQuest) continue;
            lastQuest = questString;
            if (markerBuilder.Length != 0)
                markerBuilder.Append('\n');
            markerBuilder.Append(lastQuest);
            markerBuilder.Append(">= ");
            markerBuilder.Append(i);
        }

        markerTiersLabel.text = markerBuilder.ToString();

        goal1.SetActive(questCount >= NetConstants.Account_Reward_Goal_1);
        goal2.SetActive(questCount >= NetConstants.Account_Reward_Goal_2);
        goal3.SetActive(questCount >= NetConstants.Account_Reward_Goal_3);
    }
}
