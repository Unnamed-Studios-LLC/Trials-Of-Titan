using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleQuestScaler : MonoBehaviour
{
    private void Awake()
    {
        float scale = 1;
        if (Screen.height > 1000)
            scale = 2;

        transform.localScale = new Vector3(scale, scale, scale);
    }
}
