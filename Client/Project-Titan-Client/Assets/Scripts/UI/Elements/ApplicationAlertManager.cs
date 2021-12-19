using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplicationAlertManager : MonoBehaviour
{
    public GameObject prefab;

    private void Awake()
    {
        ApplicationAlert.SetManager(this);
    }

    public ApplicationAlert Create()
    {
        var canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return null;

        var obj = Instantiate(prefab);
        obj.SetActive(true);

        obj.transform.SetParent(canvas.transform);
        obj.transform.SetAsLastSibling();

        var rectTransform = obj.GetComponent<RectTransform>();
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        return obj.GetComponent<ApplicationAlert>();
    }
}
