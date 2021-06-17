using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pooler : MonoBehaviour
{
    public GameObject objectPrefab;
    public Transform parentTransform;
    public int numInitObjects;
    private Stack<GameObject> pooledObjects;

    private void Awake()
    {
        pooledObjects = new Stack<GameObject>();
        for(int i = 0; i < numInitObjects; i++)
        {
            var obj = Instantiate(objectPrefab);
            Push(obj);
        }
    }

    public GameObject GetPooledObject()
    {
        if (pooledObjects.Count > 0)
        {
            GameObject obj = pooledObjects.Pop();
            obj.SetActive(true);
            return obj;
        }
        return Instantiate(objectPrefab);
    }

    public void Push(GameObject obj)
    {
        obj.SetActive(false);
        obj.transform.SetParent(parentTransform);
        pooledObjects.Push(obj);
    }
}