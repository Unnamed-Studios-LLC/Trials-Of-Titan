using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class FadeInMoveUp : MonoBehaviour
{
    public bool useAnchored;

    public bool runOnEnable = true;

    public float topAlpha = 1;

    private Vector3 position;

    private void OnEnable()
    {
        if (!runOnEnable) return;
        Run();
    }

    public void Run()
    {
        position = GetPosition();
        float ySpeed = Mathf.Clamp((Screen.height - position.y) / Screen.height * 0.4f, 0, float.MaxValue);
        float xSpeed = Mathf.Clamp(position.x / Screen.height * 0.08f, 0, float.MaxValue);
        transform.FadeInMoveUp(0.3f, ySpeed + xSpeed, useAnchored, topAlpha);
    }

    private Vector3 GetPosition()
    {
        if (useAnchored && transform is RectTransform rectTransform)
            return rectTransform.anchoredPosition;
        else
            return transform.position;
    }

    private void SetPosition(Vector3 position)
    {
        if (useAnchored && transform is RectTransform rectTransform)
            rectTransform.anchoredPosition = position;
        else
            transform.position = position;
    }

    private void OnDisable()
    {
        LeanTween.cancel(gameObject);
        SetPosition(position);
    }

    public void ResetPosition()
    {
        position = GetPosition();
    }

    public void ResetPosition(Vector3 position)
    {
        this.position = position;
    }
}
