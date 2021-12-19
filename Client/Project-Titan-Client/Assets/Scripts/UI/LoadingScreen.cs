using UnityEngine;
using System.Collections;
using TMPro;

public class LoadingScreen : MonoBehaviour
{
    public TextMeshProUGUI loadingText;

    public void WorldLoaded()
    {
        gameObject.SetActive(false);
    }

    public void LoadingWorld()
    {
        gameObject.SetActive(true);
    }
}
