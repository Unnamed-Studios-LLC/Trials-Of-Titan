using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GroundLabel : MonoBehaviour
{
    public TextMeshPro label;

    private WorldObject follow;

    public void Init(WorldObject toFollow, string text)
    {
        follow = toFollow;
        label.text = text;
    }

    private void LateUpdate()
    {
        var pos = follow.transform.position;
        pos.z = 0;
        transform.position = pos;

        //transform.localEulerAngles = new Vector3(0, 0, follow.world.CameraRotation);
    }
}
