// Date   : #CREATIONDATE#
// Project: #PROJECTNAME#
// Author : #AUTHOR#

using UnityEngine;
using System.Collections;

public class Crawler : Enemy
{
    
    private float MAX_MOVESPEED = 0.3f;
    private float MOVEFORCE = 200f;
    private float GROUNDED_CHECK_DIST = 0.5f;
    private float ROTATION_SPEED = 2.0f;

    private Rigidbody rb;
    private GameObject model;

    private Vector3 direction;
    bool moving = true;

    void Start()
    {
        height = 1.0f;
        rb = GetComponent<Rigidbody>();
        foreach (Transform obj in transform)
        {
            if (obj.gameObject.CompareTag("Model"))
            {
                model = obj.gameObject;
            }
        }
        rb.constraints = RigidbodyConstraints.FreezeAll & ~RigidbodyConstraints.FreezePositionX & ~RigidbodyConstraints.FreezePositionY;
        direction = transform.forward;
    }

    void Update()
    {
        if (!Physics.Raycast(transform.position + direction.normalized * 1.5f + new Vector3(0, 0.01f, 0), Vector3.down, GROUNDED_CHECK_DIST))
        {
            turn();
        }

        if (Vector3.Angle(direction, model.transform.forward) < 1)
        {
            moving = true;
        } else
        {
            Quaternion target = Quaternion.LookRotation(direction - model.transform.forward);
            model.transform.localRotation = Quaternion.RotateTowards(model.transform.localRotation, target, ROTATION_SPEED * Time.time);
            moving = false;
        }
    }

    private void FixedUpdate()
    {
        if (moving)
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
}
