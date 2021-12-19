using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SimpleButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public int touchId;

    public bool down;

    public bool held;

    public bool up;

    public void OnPointerDown(PointerEventData eventData)
    {
        down = true;
        held = true;

        touchId = GetTouchId();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        up = true;
        held = false;
    }

    private void LateUpdate()
    {
        if (down)
            down = false;

        if (up)
            up = false;
    }

    private int GetTouchId()
    {
        for (int i = 0; i < Input.touchCount; i++)
        {
            var t = Input.GetTouch(i);
            if (t.phase == TouchPhase.Began)
                return t.fingerId;
        }
        return -1;
    }
}
