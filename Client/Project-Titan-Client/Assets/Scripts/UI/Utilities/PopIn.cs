using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopIn : MonoBehaviour
{
    private void OnEnable()
    {
        transform.PopIn(0.3f, 0);
    }
}
