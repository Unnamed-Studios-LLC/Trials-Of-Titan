using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    public GameObject[] effectPrefabs;

    private Dictionary<EffectType, ObjectPool<Effect>> pools;

    private void Awake()
    {
        pools = effectPrefabs.Select(_ => new ObjectPool<Effect>(0, _)).ToDictionary(_ => _.prefab.GetComponent<Effect>().type);
    }

    public Effect GetEffect(EffectType type)
    {
        return pools[type].Get();
    }

    public void ReturnEffect(Effect effect)
    {
        pools[effect.type].Return(effect);
    }
}
