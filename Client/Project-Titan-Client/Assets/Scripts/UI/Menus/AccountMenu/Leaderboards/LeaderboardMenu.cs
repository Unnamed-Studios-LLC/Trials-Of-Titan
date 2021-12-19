using System.Collections;
using System.Collections.Generic;
using TitanCore.Core;
using TitanCore.Net.Web;
using UnityEngine;

public class LeaderboardMenu : MonoBehaviour
{
    private bool receivedResponse = false;

    private WebClient.Response<WebLeaderboardResponse> response;

    public GameObject loadingOverlay;

    public LeaderboardType leaderboardType;

    public GameObject leaderboardElementPrefab;

    public RectTransform content;

    private List<GameObject> elements = new List<GameObject>();

    private void OnEnable()
    {
        if (response == null || response.item == null || response.item.result != WebLeaderboardResult.Success)
            WebClient.SendLeaderboardDescribe(leaderboardType, ReceivedLeaderboard);
    }

    private void ReceivedLeaderboard(WebClient.Response<WebLeaderboardResponse> response)
    {
        receivedResponse = true;
        this.response = response;
    }

    private void LateUpdate()
    {
        if (receivedResponse)
        {
            loadingOverlay.SetActive(false);
            receivedResponse = false;

            if (response != null)
            {
                if (response.exception != null)
                {

                }
                else if (response.item.result == WebLeaderboardResult.Success)
                {
                    CreateLayout();
                }
                else
                {
                    CapLayout(0);
                }
            }
        }
    }

    private void CreateLayout()
    {
        var infos = response.item.leaderboard;

        for (int i = 0; i < infos.Length; i++)
        {
            var obj = GetElement(i);
            var element = obj.GetComponent<LeaderboardElement>();
            element.SetInfo(i, infos[i]);
            obj.SetActive(true);
        }

        CapLayout(infos.Length);
    }

    private GameObject GetElement(int index)
    {
        if (index < elements.Count)
        {
            return elements[index];
        }

        var obj = Instantiate(leaderboardElementPrefab);
        var rectTransform = (RectTransform)obj.transform;
        rectTransform.SetParent(content);

        var parentRect = (RectTransform)transform;
#if UNITY_IOS || UNITY_ANDROID
        float height = parentRect.rect.size.y * (1 / 5f);
#else
        float height = parentRect.rect.size.y * (1 / 9f);
#endif
        rectTransform.offsetMin = new Vector2(18, 0);
        rectTransform.offsetMax = new Vector2(-18, 0);
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, height);
        rectTransform.anchoredPosition = new Vector2(0, -height * index * 1.1f);
        elements.Add(obj);

        return obj;
    }

    private void CapLayout(int count)
    {
        for (int i = count; i < elements.Count; i++)
        {
            elements[i].SetActive(false);
        }

        if (count > 0)
        {
            var last = GetElement(count - 1);
            var lastRect = (RectTransform)last.transform;
            content.sizeDelta = new Vector2(content.sizeDelta.x, -lastRect.anchoredPosition.y + lastRect.sizeDelta.y);
        }
        else
        {
            content.sizeDelta = new Vector2(content.sizeDelta.x, 0);
        }
    }
}
