using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Data;
using TitanCore.Net.Web;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterPreview : MonoBehaviour
{
    public ClassPreview classPreview;

    public TextMeshProUGUI className;

    public ItemDisplay[] equips;

    private WebCharacterInfo info;

    public void SetCharacter(WebCharacterInfo info)
    {
        this.info = info;

        var charInfo = GameData.objects[info.type];
        GameObjectInfo skinInfo = null;
        if (info.skin != 0)
            skinInfo = GameData.objects[info.skin];
        classPreview.SetClass(skinInfo ?? charInfo);
        className.text = charInfo.name;

        for (int i = 0; i < equips.Length && i < info.equips.Length; i++)
        {
            var item = info.equips[i];
            equips[i].SetItem(item);
        }
    }

    public void OnClick()
    {
        GameManager.characterToLoad = info.id;
        GameManager.characterToCreate = 0;
        SceneManager.LoadScene(Constants.Game_Scene);
    }
}