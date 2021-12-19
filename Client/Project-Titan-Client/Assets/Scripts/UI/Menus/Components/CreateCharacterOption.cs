using System.Collections;
using System.Collections.Generic;
using TitanCore.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CreateCharacterOption : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public ClassPreview preview;

    public TextMeshProUGUI label;

    public CreateCharacterMenu menu;

    public Color questCompleteColor = Color.white;

    public Color questPendingColor = Color.white;

    public Image[] questMarkers;

    private TitanCore.Data.Entities.CharacterInfo info;

    public void SetClass(TitanCore.Data.Entities.CharacterInfo info)
    {
        this.info = info;
        preview.SetClass(info);

        label.text = info.name;
    }

    public void SetClassQuest(ClassQuest quest)
    {
        for (int i = 0; i < questMarkers.Length; i++)
        {
            if (quest.HasCompletedQuest(i))
                questMarkers[i].color = questCompleteColor;
        }
    }

    public void ClearClassQuest()
    {
        foreach (var marker in questMarkers)
            marker.color = questPendingColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        preview.animated = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        preview.animated = false;
        preview.SetClass(info);
    }

    public void OnClick()
    {
        menu.OnCreateCharacter(info);
    }
}
