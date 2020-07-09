using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Based on https://www.raywenderlich.com/847-object-pooling-in-unity
public class SmackPool : MonoBehaviour
{
    public static SmackPool Instance;
    public Object prefab;

    private List<GameObject> objects;

    void Awake() {
        if (Instance)
            Debug.LogError("Smackpool already instantiated");
        if (!prefab) {
            Debug.LogWarning("SmackPool prefab not pre-defined");
            prefab = Resources.Load("Smack");
        }
        
        Instance = this;
        objects = new List<GameObject>();
    }

    // TODO: Do this by type
    private GameObject GetFirstInactive() {
        for (int i = 0; i < objects.Count; i++) {
            if (!objects[i].activeInHierarchy) {
                return objects[i];
            }
        }
        return null;
    }


    public GameObject Create(Vector3 position, Quaternion rotation) {         
        GameObject pooledObject = GetFirstInactive();
        if (pooledObject) {
            pooledObject.SetActive(true);
        } else {
            pooledObject = (GameObject)Instantiate(prefab, position, rotation);
            objects.Add(pooledObject);
        }
        pooledObject.GetComponent<Smack>().Initiate(position, rotation);
        return pooledObject;
    }
}
