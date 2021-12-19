using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Data;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreateCharacterMenu : MonoBehaviour
{
    private const int Pc_Slots_Per_Row = 5;

    private const int Mobile_Slots_Per_Row = 6;

    public CreateCharacterOption optionPrefab;

    public LockedOption lockedOptionPrefab;

    public RectTransform content;

    public CharacterInfoMenu characterInfoMenu;

    private List<GameObject> buttons = new List<GameObject>();

    private void OnEnable()
    {
        CreateView();
    }

    private void CreateView()
    {
        foreach (var button in buttons)
            Destroy(button);
        buttons.Clear();

        optionPrefab.gameObject.SetActive(false);
        lockedOptionPrefab.gameObject.SetActive(false);

        var quests = Account.describe.classQuests.ToDictionary(_ => _.classId);

        var characters = GameData.objects.Values.Where(_ => _.Type == GameObjectType.Character && !((TitanCore.Data.Entities.CharacterInfo)_).notPlayable).OrderBy(_ => ((TitanCore.Data.Entities.CharacterInfo)_).displayOrder).ToArray();
        for (int i = 0; i < characters.Length; i++)
        {
            var info = (TitanCore.Data.Entities.CharacterInfo)characters[i];
            if (HasUnlocked(info))
            {
                var option = Instantiate(optionPrefab.gameObject).GetComponent<CreateCharacterOption>();

                var optionRect = option.GetComponent<RectTransform>();
                optionRect.SetParent(content);
                PlaceRect(optionRect, i);

                if (quests.TryGetValue(info.id, out var quest))
                    option.SetClassQuest(quest);
                else
                    option.ClearClassQuest();

                option.gameObject.SetActive(true);

                option.SetClass(info);
                buttons.Add(option.gameObject);
            }
            else
            {
                var option = Instantiate(lockedOptionPrefab.gameObject).GetComponent<LockedOption>();

                var optionRect = option.GetComponent<RectTransform>();
                optionRect.SetParent(content);
                PlaceRect(optionRect, i);

                option.SetClass(info);

                option.gameObject.SetActive(true);

                buttons.Add(option.gameObject);
            }
        }
    }

    private bool HasUnlocked(TitanCore.Data.Entities.CharacterInfo info)
    {
        foreach (var requirement in info.requirements)
        {
            bool found = false;
            foreach (var quest in Account.describe.classQuests)
            {
                if (quest.classId != (ushort)requirement.classType) continue;
                if (quest.GetCompletedCount() < requirement.questRequirement)
                    return false;
                found = true;
                break;
            }

            if (!found)
                return false;
        }
        return true;
    }

    private int GetSlotsPerRow()
    {
#if UNITY_IOS || UNITY_ANDROID
        return Mobile_Slots_Per_Row;
#else
        return Pc_Slots_Per_Row;
#endif
    }

    private Rect GetSlotRect(int index)
    {
        var slotsPerRow = GetSlotsPerRow();
        float size = 1f / slotsPerRow;
        int x = index % slotsPerRow;
        int y = index / slotsPerRow;
        return new Rect(size * x + size * 0.1f, -size * y - size * 0.1f, size * 0.8f, size * 0.8f);
    }

    private void PlaceRect(RectTransform rectTransform, int index)
    {
        var width = content.rect.width;

        var rect = GetSlotRect(index);
        rectTransform.sizeDelta = rect.size * width;
        rectTransform.anchoredPosition = rect.position * width;
    }

    public void OnCreateCharacter(TitanCore.Data.Entities.CharacterInfo info)
    {
        characterInfoMenu.gameObject.SetActive(true);
        characterInfoMenu.Show(info);
        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
