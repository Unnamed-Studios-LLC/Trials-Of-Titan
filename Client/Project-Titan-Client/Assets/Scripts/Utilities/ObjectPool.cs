using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ObjectPool<T> where T : Component
{
    public GameObject prefab;

    private Stack<T> pool = new Stack<T>();

    private int usedCount = 0;

    private Queue<float> usedHistory = new Queue<float>();

    public ObjectPool(int size, GameObject prefab)
    {
        this.prefab = prefab;

        for (int i = 0; i < size; i++)
        {
            pool.Push(Create());
        }
    }

    private T Create()
    {
        return UnityEngine.Object.Instantiate(prefab).GetComponent<T>();
    }

    private void Destroy(T obj)
    {
        UnityEngine.Object.Destroy(obj.gameObject);
    }

    public T Get()
    {
        T obj;
        if (pool.Count > 0)
            obj = pool.Pop();
        else
            obj = Create();
        obj.gameObject.SetActive(true);
        usedCount++;
        return obj;
    }

    public void Return(T obj)
    {
        obj.gameObject.SetActive(false);
        pool.Push(obj);
        usedCount--;
    }

    public void Clear()
    {
        while (pool.Count > 0)
        {
            Destroy(pool.Pop());
        }
        usedCount = 0;
    }

    public void Manage()
    {
        usedHistory.Enqueue(usedCount / (float)(usedCount + pool.Count));
        if (usedHistory.Count > 5)
            usedHistory.Dequeue();

        float max = 0;
        foreach (var count in usedHistory)
        {
            max = Mathf.Max(max, count);
        }

        if (max >= 0.5f) return;

        int toDestroy = (usedCount + pool.Count) / 2;
        for (int i = toDestroy; i > 0 && pool.Count > 0; i--)
        {
            Destroy(pool.Pop());
        }
    }
}
