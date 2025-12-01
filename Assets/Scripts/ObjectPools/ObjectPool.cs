using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public GameObject prefab;
    private Queue<GameObject> pool = new Queue<GameObject>();

    private Transform container;  // parent of pooled objects

    private void Awake()
    {
        // Create a hidden container for instances
        container = new GameObject(prefab.name + "_Container").transform;
        container.SetParent(transform);
    }

    public GameObject GetObject()
    {
        GameObject obj;

        if (pool.Count > 0)
        {
            obj = pool.Dequeue();
            obj.SetActive(true);
        }
        else
        {
            // Spawn as child of container
            obj = Instantiate(prefab, container);
        }

        return obj;
    }

    public void ReturnObject(GameObject obj)
    {
        obj.SetActive(false);

        // Re-parent object back to container
        obj.transform.SetParent(container);

        pool.Enqueue(obj);
    }
}
