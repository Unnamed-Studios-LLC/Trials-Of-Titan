using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterInfoMenu : MonoBehaviour
{
    public TextMeshProUGUI className;

    public GameObject createCharactersMenu;

    public ClassPreview classPreview;

    public TextMeshProUGUI classDescription;

    public Toggle[] quests;

    private TitanCore.Data.Entities.CharacterInfo info;

    public void Show(TitanCore.Data.Entities.CharacterInfo info)
    {
        this.info = info;

        classPreview.SetClass(info);
        className.text = $"- {info.name} -";
        classDescription.text = info.description;

        var questProgression = Account.GetClassQuest(info.id);
        for (int i = 0; i < quests.Length; i++)
        {
            quests[i].isOn = questProgression.HasCompletedQuest(i);
        }
    }

    public void Back()
    {
        gameObject.SetActive(false);
        createCharactersMenu.SetActive(true);
    }

    public void Play()
    {
        GameManager.characterToCreate = info.id;
        GameManager.characterToLoad = 0;
        SceneManager.LoadScene(Constants.Game_Scene);
    }
}
