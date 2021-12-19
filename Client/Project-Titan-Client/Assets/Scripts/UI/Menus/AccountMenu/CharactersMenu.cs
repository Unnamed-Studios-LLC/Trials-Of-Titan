using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Net;
using TitanCore.Net.Web;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharactersMenu : MonoBehaviour
{
    private const int Pc_Slots_Per_Row = 5;

    private const int Mobile_Slots_Per_Row = 5;

    public GameObject characterPrefab;

    public GameObject createCharacterPrefab;

    public RectTransform purchaseCharacter;

    public GameObject overlay;

    public RectTransform scrollContent;

    public TextMeshProUGUI currencyLabel;

    private GameObject[] characters;

    private GameObject[] createCharacters;

    private WebClient.Response<WebPurchaseSlotResponse> purchaseResponse;

    private void OnEnable()
    {
        CreateView();
    }

    private void CreateView()
    {
        if (characters != null)
        {
            for (int i = 0; i < characters.Length; i++)
                Destroy(characters[i]);
        }

        if (createCharacters != null)
        {
            for (int i = 0; i < createCharacters.Length; i++)
                Destroy(createCharacters[i]);
        }

        ((RectTransform)transform).ForceUpdateRectTransforms();
        characters = new GameObject[Account.describe.characters.Length];
        for (int i = 0; i < characters.Length; i++)
        {
            var charInfo = Account.describe.characters[i];
            var character = CreateCharacterPreview();
            var charRect = character.GetComponent<RectTransform>();
            PlaceRect(charRect, i);
            character.SetActive(true);
            character.GetComponent<CharacterPreview>().SetCharacter(charInfo);
            characters[i] = character;
        }

        int index;
        createCharacters = new GameObject[Account.describe.maxCharacters - characters.Length];
        for (index = characters.Length; index < Account.describe.maxCharacters; index++)
        {
            var create = CreateCharacterCreate();
            var createRect = create.GetComponent<RectTransform>();
            PlaceRect(createRect, index);
            create.SetActive(true);
            createCharacters[index - characters.Length] = create;
        }

        purchaseCharacter.gameObject.SetActive(false);
        PlaceRect(purchaseCharacter, index);
        purchaseCharacter.gameObject.SetActive(true);
        purchaseCharacter.GetComponent<PurchaseSlot>().SetPrice(NetConstants.GetCharacterSlotCost(Account.describe.maxCharacters));

        scrollContent.sizeDelta = new Vector2(scrollContent.sizeDelta.x, -purchaseCharacter.anchoredPosition.y + purchaseCharacter.sizeDelta.y);

        UpdateCurrencyLabel();
    }

    private GameObject CreateCharacterPreview()
    {
        var character = Instantiate(characterPrefab);
        character.transform.SetParent(characterPrefab.transform.parent);
        return character;
    }

    private GameObject CreateCharacterCreate()
    {
        var create = Instantiate(createCharacterPrefab);
        create.transform.SetParent(createCharacterPrefab.transform.parent);
        return create;
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
        var width = scrollContent.rect.width;

        var rect = GetSlotRect(index);
        rectTransform.sizeDelta = rect.size * width;
        rectTransform.anchoredPosition = rect.position * width;
    }

    public void OnPurchaseSlot()
    {
        var cost = NetConstants.GetCharacterSlotCost(Account.describe.maxCharacters);
        if (Account.describe.currency < cost)
        {
            Debug.Log("Not enough gold to purchase slot");
            return;
        }

        overlay.SetActive(true);
        WebClient.SendPurchaseSlot(Account.savedAccessToken, OnPurchaseSlotResponse);
    }

    private void OnPurchaseSlotResponse(WebClient.Response<WebPurchaseSlotResponse> response)
    {
        purchaseResponse = response;
    }

    private void Update()
    {
        if (purchaseResponse != null)
        {
            overlay.SetActive(false);
            var response = purchaseResponse;
            purchaseResponse = null;

            if (response.item == null || response.item.result != WebPurchaseSlotResult.Success)
            {

            }
            else
            {
                Account.describe.currency = response.item.currency;
                Account.describe.maxCharacters++;
                purchaseCharacter.GetComponent<PurchaseSlot>().SetPrice(NetConstants.GetCharacterSlotCost(Account.describe.maxCharacters));
                CreateView();
            }
        }
    }

    private void UpdateCurrencyLabel()
    {
        currencyLabel.text = Constants.Premium_Currency_Sprite + Account.describe.currency;
    }
}
