using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectShadow : MonoBehaviour
{
    public Transform toFollow;

    private Option drawShadows;

    private void Awake()
    {
        drawShadows = Options.Get(OptionType.DrawShadows);
    }

    public void Enable()
    {
        drawShadows.AddBoolCallback(OnDrawShadowsChanged);
        SetVisibility(drawShadows.GetBool());
    }

    public void Disable()
    {
        drawShadows.RemoveBoolCallback(OnDrawShadowsChanged);
    }

    private void OnDestroy()
    {
        drawShadows.RemoveBoolCallback(OnDrawShadowsChanged);
    }

    private void OnDrawShadowsChanged(bool value)
    {
        SetVisibility(value);
    }

    private void SetVisibility(bool visible)
    {
        gameObject.SetActive(visible);
    }

    public void SetToFollow(Transform toFollow)
    {
        this.toFollow = toFollow;
        Position();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Position();
    }

    private void Position()
    {
        var pos = toFollow.position;
        pos.z = 0;
        transform.position = pos;
    }
}
