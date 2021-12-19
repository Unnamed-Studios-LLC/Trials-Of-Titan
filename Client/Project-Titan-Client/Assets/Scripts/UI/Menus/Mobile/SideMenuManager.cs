using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Mobile
{
    public class SideMenuManager : MonoBehaviour
    {
        private enum AnimationState
        {
            Closed,
            Open,
            Opening,
            Closing
        }

        public SideMenu[] sideMenus;

        public RectTransform rectTransform;

        private SideMenu currentOpen;

        private AnimationState state = AnimationState.Closed;

        private LTSeq currentTween;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();

            for (int i = 0; i < sideMenus.Length; i++)
            {
                var menu = sideMenus[i];
                menu.SizeButton(i);
                menu.SetHidden();
            }
        }

        public void ToggleMenu(SideMenu menu)
        {
            if (currentOpen == menu) CloseMenu();
            else OpenMenu(menu);
        }

        public void SetButtonOffsets(float offset)
        {
            foreach (var menu in sideMenus)
            {
                menu.SetButtonOffset(offset);
                if (currentOpen == null)
                {
                    menu.ReturnButton();
                }
                else if (menu == currentOpen)
                {
                    if (state == AnimationState.Open || state == AnimationState.Opening)
                        menu.ExpandButton();
                    else
                        menu.ReturnButton();
                }
                else if (state == AnimationState.Closing)
                {
                    menu.ReturnButton();
                }
            }
        }

        public void OpenMenu(SideMenu menu)
        {
            if (currentOpen == menu) return;
            if (currentTween != null) return;
            currentTween = LeanTween.sequence();
            if (currentOpen != null)
            {
                currentOpen.TweenToHide(currentTween);
            }

            foreach (var otherMenu in sideMenus)
            {
                if (otherMenu == menu) continue;
                otherMenu.HideButton();
            }

            currentOpen = menu;
            state = AnimationState.Opening;
            menu.TweenToShow(currentTween);
            currentTween.append(() =>
            {
                currentTween = null;
                state = AnimationState.Open;
            });
        }

        public void CloseMenu()
        {
            if (currentTween != null || currentOpen == null) return;

            foreach (var otherMenu in sideMenus)
            {
                if (otherMenu == currentOpen) continue;
                otherMenu.ReturnButton();
            }

            state = AnimationState.Closing;
            currentTween = LeanTween.sequence();
            currentOpen.TweenToHide(currentTween);
            currentTween.append(() =>
            {
                currentTween = null;
                currentOpen = null;
                state = AnimationState.Closed;
            });
        }

        public void DamageTaken()
        {
            if (currentOpen == null) return;
            if (!currentOpen.closeOnDamage) return;
            CloseMenu();
        }
    }
}
