using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TabNavigation : MonoBehaviour
{
    public Selectable[] selectables;

    private void LateUpdate()
    {
        if (!Input.GetKeyDown(KeyCode.Tab) || selectables == null) return;
        for (int i = 0; i < selectables.Length; i++)
        {
            var s = selectables[i];
            if (EventSystem.current.currentSelectedGameObject != s.gameObject) continue;
            for (int j = 1; j < selectables.Length; j++)
            {
                var next = selectables[(i + j) % selectables.Length];
                if (!next.interactable) continue;
                EventSystem.current.SetSelectedGameObject(next.gameObject);
                return;
            }
        }
    }
}
