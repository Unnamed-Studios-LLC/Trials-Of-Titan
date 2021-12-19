using Mobile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class LootMenu : SideMenu
{
    public MobileSlotOrganizer slotOrganizer;

    public SideMenuManager sideMenuManager;

    public SideMenu inventoryMenu;

    private bool showing = false;

    private bool moving = false;

    private LTSeq tweenSeq;

    private void Awake()
    {
        slotOrganizer.Size();
        gameObject.SetActive(false);
        rectTransform.anchoredPosition = GetHiddenPosition();
    }

    public void Show()
    {
        if (showing) return;
        sideMenuManager.SetButtonOffsets(-rectTransform.rect.width);
        showing = true;
        moving = true;
        LeanTween.cancel(gameObject);
        var seq = LeanTween.sequence();
        seq.append(() => gameObject.SetActive(true));
        seq.append(rectTransform.LeanMove(new Vector3(GetX(), 0, 0), Menu_Animation_Time).setEaseOutSine());
        seq.append(() =>
        {
            moving = false;
        });
        //TweenToShow(seq);
    }

    public void Hide()
    {
        if (!showing) return;
        sideMenuManager.SetButtonOffsets(0);
        showing = false;
        moving = true;
        LeanTween.cancel(gameObject);
        var seq = LeanTween.sequence();
        seq.append(rectTransform.LeanMove((Vector3)GetHiddenPosition(), Menu_Animation_Time).setEaseInSine());
        seq.append(() => gameObject.SetActive(false));
        seq.append(() =>
        {
            moving = false;
        });
    }

    private void LateUpdate()
    {
        if (moving) return;
        rectTransform.anchoredPosition = new Vector2(GetX(), 0);
    }

    private float GetX()
    {
        return inventoryMenu.rectTransform.anchoredPosition.x - inventoryMenu.rectTransform.rect.width + 6;
    }
}
