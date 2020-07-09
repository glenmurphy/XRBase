using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Smack : MonoBehaviour
{   
    // Config
    public GameObject burst2;
    private float lifeTime = 0.12f;
    private float ageScale = 3f;
    private float initialSize = 0.8f;
    private float baseSize = 2f;
    private float randomExtraSize = 2.4f;

    // Working vars
    private float age;
    private float scale;
    private float extraSize;

    // As this is a pooled object, we can't use Start or Awake();
    void OnEnable()
    {
        age = 0;
        extraSize = Random.value * randomExtraSize;
        scale = initialSize + extraSize;
        this.transform.localScale = new Vector3(scale, scale, scale);
    }

    public void Initiate(Vector3 position, Quaternion rotation)
    {
        gameObject.SetActive(true);
        transform.position = position;
        transform.rotation = rotation;
        burst2.transform.rotation = Random.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (age > lifeTime) {
            gameObject.SetActive(false);
            return;
        }

        if (age > 0) {
            scale = age * ageScale + baseSize + extraSize;   
            this.transform.localScale = new Vector3(scale, scale, scale);
        }
        age += Time.deltaTime;
    }
}
