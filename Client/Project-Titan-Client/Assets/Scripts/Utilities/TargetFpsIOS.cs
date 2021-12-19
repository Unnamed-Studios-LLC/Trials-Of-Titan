using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetFpsIOS : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
#if UNITY_IOS
        Application.targetFrameRate = 60;
#endif
    }
}
