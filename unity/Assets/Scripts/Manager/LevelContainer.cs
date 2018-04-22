// Date   : #CREATIONDATE#
// Project: #PROJECTNAME#
// Author : #AUTHOR#

using UnityEngine;
using System.Collections;

public class LevelContainer : MonoBehaviour {

    [SerializeField]
    GameObject bgTemplate;

    [SerializeField]
    GameObject cloudTemplate;

    [SerializeField]
    float levelWidth;

    void Start () {
        float bgElemWidth = 20f;
        for (float w = 0; w < levelWidth; w += bgElemWidth)
        {
            GameObject bgElem = Instantiate(bgTemplate, transform);
            bgElem.transform.position = new Vector3(w, bgElem.transform.position.y, bgElem.transform.position.z);

            for (var c = 0; c < 2; c++)
            {
                GameObject cloud = Instantiate(cloudTemplate, transform);
                Vector3 offset = new Vector3(Random.Range(-10, 30), Random.Range(10, 50), Mathf.Round(Random.Range(10, 60)) + 0.5f);
                cloud.transform.position = bgElem.transform.position + offset;
            }
        }
        transform.Translate(new Vector3(-levelWidth / 2, 0, 0));
    }

    void Update () {
    
    }
}
