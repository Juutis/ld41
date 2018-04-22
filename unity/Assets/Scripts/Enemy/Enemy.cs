// Date   : #CREATIONDATE#
// Project: #PROJECTNAME#
// Author : #AUTHOR#

using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour {

    public float height = 1.0f;

    [SerializeField]
    private GameObject deathSplat;

    void Start () {
    
    }

    void Update () {
    
    }

    void FixedUpdate()
    {
    }

    public void Die()
    {
        GameObject obj = Instantiate(deathSplat);
        obj.transform.parent = transform.parent;
        obj.transform.position = transform.position;
        Destroy(obj, 20.0f);
        gameObject.SetActive(false);
    }
}
