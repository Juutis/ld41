
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Level : MonoBehaviour
{

    public void Load()
    {
    }

    public void Unload()
    {
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
    
}
