// Date   : #CREATIONDATE#
// Project: #PROJECTNAME#
// Author : #AUTHOR#

using UnityEngine;
using System.Collections;

public class FireBall : MonoBehaviour {

    float FORCE = 1000.0f;
    float GRACE_PERIOD = 0.1f;
    float MAX_LIFETIME = 10.0f;

    public Enemy shooter;

    Rigidbody rb;
    ParticleSystem ps;
    float startTime;
    
    [SerializeField]
    private AudioClip hitSound;
    AudioSource audioSource;

    void Start ()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        ps = gameObject.GetComponentInChildren<ParticleSystem>();
        rb.AddForce(transform.forward * FORCE);
        startTime = Time.time;
    }
    
    void Update () {

        if (Time.time - startTime > MAX_LIFETIME)
        {
            Die();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.isTrigger && Time.time - startTime > GRACE_PERIOD)
        {
            PlayHitSound();
            Die();
        }
    }

    void Die()
    {
        ps.transform.parent = null;
        ps.Stop();
        Destroy(ps.gameObject, 5.0f);
        Destroy(gameObject, 0.5f);
    }
    
    private void PlayHitSound()
    {
        audioSource.clip = hitSound;
        audioSource.Play();
    }
}
