using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class LockedOption : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public ClassPreview preview;

    public TextMeshProUGUI label;

    public QuestRequirement requirement;

    private TitanCore.Data.Entities.CharacterInfo info;

    public void SetClass(TitanCore.Data.Entities.CharacterInfo info)
    {
        this.info = info;
        preview.SetClass(info);

        label.text = info.name;

        CreateRequirements();
    }

    private void CreateRequirements()
    {
        requirement.gameObject.SetActive(false);

        for (int i = 0; i < info.requirements.Length; i++)
        {
            var classReq = info.requirements[i];
            var req = Instantiate(requirement.gameObject).GetComponent<QuestRequirement>();
            req.transform.SetParent(requirement.transform.parent);
            req.transform.position = requirement.transform.position;

            var rectT = (RectTransform)req.transform;
            rectT.offsetMin = Vector2.zero;
            rectT.offsetMax = Vector2.zero;


            req.transform.localPosition += new Vector3(0, ((RectTransform)requirement.transform).rect.height * i * 1.2f, 0);

            req.gameObject.SetActive(true);
            req.SetRequirement(classReq);
        }
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
}
