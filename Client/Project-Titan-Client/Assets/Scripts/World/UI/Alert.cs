using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Alert : MonoBehaviour
{
    public TextMeshPro label;

    private MeshRenderer meshRenderer;

    private WorldObject owner;

    public uint ownerId;

    private Vector3 lastOwnerPosition;

    private float height = 0f;

    private float lastHeight;

    private float time;

    private ObjectManager objectManager;

    private Camera mainCamera;

    private static int sort = int.MinValue;

    private bool statusEffect = false;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    private void PositionToOwner()
    {
        Vector3 ownerPosition;
        float objHeight;
        if (owner == null || owner.gameObject == null || owner.gameId != ownerId)
        {
            owner = null;
            ownerPosition = lastOwnerPosition;
            objHeight = lastHeight;
        }
        else
        {
            ownerPosition = owner.transform.localPosition;
            if (owner is Object3d)
                ownerPosition += new Vector3(0.5f, 0.5f, 0);
            objHeight = owner.GetHeight();
            lastHeight = objHeight;
        }

        var position = ownerPosition + new Vector3(0, 0, -height - objHeight - (statusEffect ? 0.5f : 0));
        transform.localPosition = position;
        lastOwnerPosition = ownerPosition;
    }

    private void LateUpdate()
    {
        if (height < 1 || owner == null || owner.gameObject  == null)
            height += Time.deltaTime / 2;

        PositionToOwner();
        time += Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(mainCamera.transform.up, -mainCamera.transform.forward);
        if (time > 1)
        {
            objectManager.ReturnAlert(this);
        }
    }

    public void Init(World world, WorldObject owner, string text, Color color, ObjectManager objectManager, bool statusEffect)
    {
        this.statusEffect = statusEffect;

        LeanTween.cancel(gameObject);
        transform.localScale = Vector3.one;

        mainCamera = world.worldCamera;
        this.owner = owner;
        ownerId = owner.gameId;
        this.objectManager = objectManager;
        label.text = text;
        label.color = color;
        label.sortingOrder = sort++;

        time = 0;
        lastHeight = 0;
        height = 0;
        lastOwnerPosition = Vector3.zero;
    }

    public void UpdateText(string text)
    {
        label.text = text;
        label.sortingOrder = sort++;
        time = 0;

        if (height > 0.5f)
            height = 0.5f;

        LeanTween.cancel(gameObject);
        transform.localScale = Vector3.one;
        LeanTween.sequence()
            .append(LeanTween.scale(gameObject, new Vector3(2, 2, 2), 0.15f).setEaseInSine())
            .append(LeanTween.scale(gameObject, Vector3.one, 0.15f).setEaseOutSine());
    }

    private void OnDisable()
    {
        owner = null;
        ownerId = 0;
    }
}
