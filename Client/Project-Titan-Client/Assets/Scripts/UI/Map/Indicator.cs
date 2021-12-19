using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Indicator : MonoBehaviour
{
    public Color originalColor;

    public WorldObject obj;

    public Sprite circleSprite;

    public SpriteRenderer spriteRenderer;

    public float sizeAdjustment = 1;

    private void OnEnable()
    {
        UpdateSize();
    }

    private void OnDisable()
    {
        spriteRenderer.sprite = circleSprite;
        spriteRenderer.color = originalColor;
    }

    private void Start()
    {
        UpdateSize();
    }

    public void UpdateSize()
    {
        if (obj != null && obj.world != null)
            SetSize(obj.world.minimapCamera.orthographicSize);
    }

    public void SetSize(float orthoSize)
    {
        float size = orthoSize * 0.4f * sizeAdjustment;
        var objSize = obj.GetVisualSize();
        if (objSize == 0)
            objSize = 1;
        transform.localScale = new Vector3(size / objSize, size / objSize, size / objSize);
    }

    private void LateUpdate()
    {
        if (obj != null && obj.world != null && obj.info.Type == TitanCore.Data.GameObjectType.Enemy)
        {
            var rot = transform.eulerAngles;
            rot.z = obj.world.CameraRotation;
            transform.eulerAngles = rot;
        }
    }
}
