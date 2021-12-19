using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    [Serializable]
    public struct OptionPage
    {
        public Image tabButton;

        public GameObject page;
    }

    public Image currentPage;

    public Sprite selectedTabSprite;

    public Sprite defaultTabSprite;

    public OptionPage[] pages;

    private Dictionary<Image, GameObject> pagesDict;

    private void Awake()
    {
        pagesDict = pages.ToDictionary(_ => _.tabButton, _ => _.page);
        foreach (var page in pages)
            if (page.page != null)
                page.page.SetActive(false);
        SelectPage(currentPage);
    }

    public void SelectPage(Image tab)
    {
        if (currentPage != null)
        {
            currentPage.sprite = defaultTabSprite;
            SetPageActive(currentPage, false);
        }

        tab.sprite = selectedTabSprite;
        SetPageActive(tab, true);
        currentPage = tab;
    }

    private void SetPageActive(Image tab, bool active)
    {
        if (!pagesDict.TryGetValue(tab, out var page)) return;
        if (page == null) return;
        page.gameObject.SetActive(active);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Close()
    {
        Options.Save();
        gameObject.SetActive(false);
    }
}
