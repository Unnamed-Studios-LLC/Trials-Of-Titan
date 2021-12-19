using System.Collections;
using System.Collections.Generic;
using TitanCore.Core;
using UnityEngine;

public class WorldHealthBar : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;

    public Transform bar;

    public Character owner;

    // Update is called once per frame
    void LateUpdate()
    {
        float percent = 1;
        if (owner is Player player)
            percent = player.health / (float)owner.GetStatFunctional(StatType.MaxHealth);
        else
            percent = owner.serverHealth / (float)owner.GetStatFunctional(StatType.MaxHealth);
        percent = Mathf.Clamp01(percent);
        bar.localScale = new Vector3(percent, 1, 1);

        transform.rotation = owner.transform.rotation;
        var pos = owner.transform.position;
        pos.z = 0;
        pos += new Vector3(-0.6f, 0, owner.HasGroundLabel() ? 0.6f : 0.27f);
        transform.position = pos;
    }
}
