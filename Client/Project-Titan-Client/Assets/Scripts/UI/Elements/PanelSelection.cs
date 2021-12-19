using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PanelSelection : MonoBehaviour
{
    [Serializable]
    public struct Panel
    {
        public GameObject gameObject;

        public Graphic selectable;
    }

    public GameObject defaultPanel;

    public Panel[] panels;

    public Color selectedColor = Color.white;

    public Color normalColor = Color.white;

    private GameObject selectedPanel;

    private Dictionary<GameObject, Graphic> selectables;

    private void Start()
    {
        selectables = panels.ToDictionary(_ => _.gameObject, _ => _.selectable);
        foreach (var s in selectables.Values)
            Deselect(s);
        foreach (var panel in selectables.Keys)
            panel.SetActive(false);
        SelectPanel(defaultPanel);
    }

    public void SelectPanel(GameObject panel)
    {
        if (selectedPanel == panel) return;

        if (selectedPanel != null)
        {
            Deselect(selectables[selectedPanel]);
            selectedPanel.SetActive(false);
        }

        Select(selectables[panel]);
        panel.SetActive(true);

        selectedPanel = panel;
    }

    private void Deselect(Graphic graphic)
    {
        graphic.color = normalColor;
    }

    private void Select(Graphic graphic)
    {
        graphic.color = selectedColor;
    }
}
