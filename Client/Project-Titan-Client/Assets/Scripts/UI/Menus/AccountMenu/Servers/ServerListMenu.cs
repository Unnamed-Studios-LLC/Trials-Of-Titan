using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ServerListMenu : MonoBehaviour
{
    public RectTransform content;

    public GameObject entryPrefab;

    private List<ServerEntry> entries = new List<ServerEntry>();

    private void OnEnable()
    {
        Layout();
    }

    private void Layout()
    {
        foreach (var entry in entries)
        {
            Destroy(entry.gameObject);
        }
        entries.Clear();

        var selected = Account.GetSelectedServer();

        float spacing = ((RectTransform)transform).rect.height / 8;
        float size = spacing * 0.9f;
        CreateBest(size, selected == null);
        for (int i = 0; i < Account.describe.servers.Length; i++)
        {
            var serverInfo = Account.describe.servers[i];

            var entryGameObject = CreateEntry(-spacing * entries.Count, size);
            var entry = entryGameObject.GetComponent<ServerEntry>();
            entry.Setup(serverInfo);
            entry.SetSelected(selected == serverInfo);
            entryGameObject.SetActive(true);

            entries.Add(entry);
        }

        content.sizeDelta = new Vector2(content.sizeDelta.x, spacing * entries.Count);
    }

    private void CreateBest(float size, bool selected)
    {
        var entryGameObject = CreateEntry(0, size);
        var entry = entryGameObject.GetComponent<ServerEntry>();
        entry.SetupBest();
        entry.SetSelected(selected);
        entryGameObject.SetActive(true);

        entries.Add(entry);
    }

    private GameObject CreateEntry(float position, float size)
    {
        var entryGameObject = Instantiate(entryPrefab);
        var rectTransform = (RectTransform)entryGameObject.transform;
        rectTransform.SetParent(content);
        rectTransform.anchoredPosition = new Vector2(0, position);
        rectTransform.offsetMin = new Vector2(0, rectTransform.offsetMin.y);
        rectTransform.offsetMax = new Vector2(0, rectTransform.offsetMax.y);
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, size);
        return entryGameObject;
    }

    public void Select(ServerEntry selectedEntry)
    {
        foreach (var entry in entries)
        {
            if (entry != selectedEntry)
            {
                entry.SetSelected(false);
                continue;
            }

            entry.SetSelected(true);
            Account.SetSelectedServer(entry.webInfo);
        }
    }
}
