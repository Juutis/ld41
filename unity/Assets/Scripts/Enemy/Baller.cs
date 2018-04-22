// Date   : #CREATIONDATE#
// Project: #PROJECTNAME#
// Author : #AUTHOR#

using UnityEngine;
using System.Collections;

public class Baller : Enemy
{

    private float MAX_MOVESPEED = 0.15f;
    private float MOVEFORCE = 100f;
    private float GROUNDED_CHECK_DIST = 0.5f;
    private float ROTATION_SPEED = 1.0f;
    private float COOLDOWN = 1.0f;
    private float SHOOT_DELAY = 0.4f;
    private float SHOOT_DURATION = 1.2f;
    Vector3 FIREBALL_OFFSET = new Vector3(1.0f, 1.3f, 0);

    Rigidbody rb;
    GameObject model;
    Animator anim;

    [SerializeField]
    private GameObject fireball;
    GameObject player;

    private Vector3 direction;

    enum Status { Moving, Shooting, Rotating };

    Status status = Status.Moving;

    float nextFireball = 0;
    float startMovingAfterShoot = 0;

    [SerializeField]
    private AudioClip shootSound;
    AudioSource audioSource;


    void Start()
    {
        height = 2.0f;
        rb = GetComponent<Rigidbody>();
        foreach (Transform obj in transform)
        {
            if (obj.gameObject.CompareTag("Model"))
            {
                model = obj.gameObject;
            }
        }
        anim = model.GetComponent<Animator>();
        player = GameObject.FindGameObjectsWithTag("Player")[0];
        rb.constraints = RigidbodyConstraints.FreezeAll & ~RigidbodyConstraints.FreezePositionX & ~RigidbodyConstraints.FreezePositionY;
        direction = transform.forward;

        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (!Physics.Raycast(transform.position + direction.normalized * 1.5f + new Vector3(0, 0.01f, 0), Vector3.down, GROUNDED_CHECK_DIST))
        {
            turn();
        }

        if (Vector3.Angle(direction, model.transform.forward) < 1)
        {
            if (status == Status.Rotating)
            {
                status = Status.Moving;
            }
        }
        else
        {
            Quaternion target = Quaternion.LookRotation(direction - model.transform.forward);
            model.transform.localRotation = Quaternion.RotateTowards(model.transform.localRotation, target, ROTATION_SPEED * Time.time);
            if (status == Status.Moving)
            {
                status = Status.Rotating;
            }
        }

        if (Time.time > nextFireball)
        {

            if (status == Status.Moving && facingPlayer() && Vector3.Distance(transform.position, player.transform.position) < 50f)
            {
                status = Status.Shooting;
                nextFireball = Time.time + SHOOT_DELAY;
                startMovingAfterShoot = Time.time + SHOOT_DURATION;
                anim.SetTrigger("toShoot");
            }
            else
            {
                if (status == Status.Shooting)
                {
                    Vector3 pos = transform.position + direction.normalized * FIREBALL_OFFSET.x + new Vector3(0, FIREBALL_OFFSET.y, FIREBALL_OFFSET.z);
                    Quaternion rot = Quaternion.LookRotation(direction + new Vector3(0, 0.5f, 0));
                    GameObject proj = Instantiate(fireball, pos, rot);
                    proj.GetComponent<FireBall>().shooter = this;
                    nextFireball = Time.time + 1000f;
                    PlayShootSound();
                }
            }
        }
        if (Time.time > startMovingAfterShoot && status == Status.Shooting)
        {
            anim.SetTrigger("toIdle");
            status = Status.Moving;
            nextFireball = Time.time + COOLDOWN;
        }
        
    }

    bool facingPlayer()
    {
        return Mathf.Sign(player.transform.position.x - transform.position.x) == Mathf.Sign(direction.x);
    }

    private void FixedUpdate()
    {
        if (status == Status.Moving)
        {
            rb.AddForce(direction * MOVEFORCE);
        }
        else
        {
            rb.velocity = new Vector3(0, rb.velocity.y, rb.velocity.z);
        }

        if (Mathf.Abs(rb.velocity.x) > MAX_MOVESPEED)
        {
            rb.velocity = new Vector3(Mathf.Sign(rb.velocity.x) * MAX_MOVESPEED, rb.velocity.y, rb.velocity.z);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        turn();
    }

    void OnTriggerEnter(Collider other)
    {
        FireBall fireBall = other.gameObject.GetComponent<FireBall>();
        if (fireBall != null)
        {
            if (fireBall.shooter != this)
            {
                Die();
            }
        }
    }

    void turn()
    {
        direction = direction * -1;
    }

    private void PlayShootSound()
    {
        audioSource.clip = shootSound;
        audioSource.Play();
    }
}
