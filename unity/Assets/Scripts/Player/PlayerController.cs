using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Utility;
using Random = UnityEngine.Random;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class PlayerController : MonoBehaviour
{
    float ABSOLUTE_MAX_MOVESPEED = 15.0f;
    float MOVESPEED_INCREMENT = 0.7f;

    private float GROUNDED_CHECK_DIST = 0.1f;
    private float MOVE_CHECK_DIST = 0.1f;
    private float MAX_MOVESPEED = 0.7f;
    private float MAX_CLIMBSPEED = 0.5f;
    private float MAX_JUMPSPEED = 12f;
    private float MOVESPEED = 1.0f;
    private float MOVEFORCE = 600.0f;
    private float CLIMBSPEED = 1.0f;
    private float JUMPFORCE = 600.0f;
    private float JUMP_GRACE_PERIOD = 0.2f;
    float DEATH_PLANE = -10;

    private float jump_timeout = 0;
    float ladder_detach_timeout = 0;

    float max_movespeed;
    float last_movespeed = 0;
    
    private bool onLadder = false;
    private int ladderCount = 0;

    [SerializeField]
    [Range(0f, 1f)]
    private float m_RunstepLenghten;
    [SerializeField]
    private float m_JumpSpeed;
    [SerializeField]
    private float m_StickToGroundForce;
    [SerializeField]
    private float m_GravityMultiplier;
    [SerializeField]
    private MouseLook m_MouseLook;
    [SerializeField]
    private bool m_UseFovKick;
    [SerializeField]
    private FOVKick m_FovKick = new FOVKick();
    [SerializeField]
    private bool m_UseHeadBob;
    [SerializeField]
    private CurveControlledBob m_HeadBob = new CurveControlledBob();
    [SerializeField]
    private LerpControlledBob m_JumpBob = new LerpControlledBob();
    [SerializeField]
    private float m_StepInterval;
    [SerializeField]
    private AudioClip[] m_FootstepSounds;    // an array of footstep sounds that will be randomly selected from.
    [SerializeField]
    private AudioClip m_JumpSound;           // the sound played when character leaves the ground.
    [SerializeField]
    private AudioClip m_LandSound;           // the sound played when character touches back on ground.

    [SerializeField]
    private GameObject deathSplat;

    private Camera m_Camera;
    private bool m_Jump;
    private float m_YRotation;
    private Vector2 m_Input;
    private Vector3 m_MoveDir = Vector3.zero;
    private CollisionFlags m_CollisionFlags;
    private bool m_PreviouslyGrounded;
    private Vector3 m_OriginalCameraPosition;
    private float m_StepCycle;
    private float m_NextStep;
    private bool m_Jumping;
    private AudioSource m_AudioSource;

    private bool isGrounded = false;
    private Rigidbody rb;

    bool alive = true;

    GameManager main;

    float steppedOnTimer = 0;

    List<Vector3> velocities = new List<Vector3>();

    // Use this for initialization
    private void Start()
    {
        m_Camera = Camera.main;
        m_OriginalCameraPosition = m_Camera.transform.localPosition;
        m_FovKick.Setup(m_Camera);
        m_HeadBob.Setup(m_Camera, m_StepInterval);
        m_StepCycle = 0f;
        m_NextStep = m_StepCycle / 2f;
        m_Jumping = false;
        m_AudioSource = GetComponent<AudioSource>();
        m_MouseLook = GetComponentInChildren<MouseLook>();

        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeAll & ~RigidbodyConstraints.FreezePositionX & ~RigidbodyConstraints.FreezePositionY;

        main = GameObject.FindGameObjectsWithTag("GameController")[0].GetComponent<GameManager>();

        max_movespeed = MAX_MOVESPEED;
    }


    // Update is called once per frame
    private void Update()
    {
        if (transform.position.y < DEATH_PLANE)
        {
            Die();
        }
        
        if (Physics.Raycast(transform.position + new Vector3(0.5f, 0.01f), Vector3.down, GROUNDED_CHECK_DIST, levelGeomLayerMask) |
            Physics.Raycast(transform.position + new Vector3(-0.5f, 0.01f), Vector3.down, GROUNDED_CHECK_DIST, levelGeomLayerMask) |
            Physics.Raycast(transform.position + new Vector3(0f, 0.01f), Vector3.down, GROUNDED_CHECK_DIST, levelGeomLayerMask))
        {
            isGrounded = true;
            if (rb.velocity.y < 0)
            {
                rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            }
            max_movespeed = MAX_MOVESPEED;
        }
        else
        {
            isGrounded = false;
        }

        // the jump state needs to read here to make sure it is not missed
        if (CrossPlatformInputManager.GetButtonDown("Jump"))
        {
            m_Jump = true;
            jump_timeout = Time.time + JUMP_GRACE_PERIOD;
        }

        if (m_Jump && jump_timeout < Time.time)
        {
            m_Jump = false;
        }

        if (!m_PreviouslyGrounded && isGrounded)
        {
            StartCoroutine(m_JumpBob.DoBobCycle());
            PlayLandingSound();
            m_MoveDir.y = 0f;
            m_Jumping = false;
            PlayFootStepAudio();
        }
        if (!isGrounded && !m_Jumping && m_PreviouslyGrounded)
        {
            m_MoveDir.y = 0f;
        }

        m_PreviouslyGrounded = isGrounded;

        if (!alive)
        {
            if(Input.anyKeyDown)
            {
                main.triggerRestart();
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            main.triggerRestart();
        }
    }


    private void PlayLandingSound()
    {
        m_AudioSource.clip = m_LandSound;
        m_AudioSource.Play();
        m_NextStep = m_StepCycle + .5f;
    }

    private float getAvgXVelocity()
    {
        if (velocities.Count == 0)
        {
            return rb.velocity.x;
        }

        float result = 0, i = 0;
        foreach (Vector3 v in velocities)
        {
            result += v.x;
            i += 1.0f;
        }
        return result / i;
    }

    private float getAvgYVelocity()
    {
        if (velocities.Count == 0)
        {
            return rb.velocity.y;
        }
        float result = 0, i = 0;
        foreach (Vector3 v in velocities)
        {
            result += v.y;
            i += 1.0f;
        }
        return result / i;
    }

    private void FixedUpdate()
    {
        float cur_ms = getAvgXVelocity();

        if (onLadder && Math.Sign(cur_ms) != Math.Sign(last_movespeed) || Math.Abs(cur_ms) < 0.01f)
        {
            max_movespeed = MAX_MOVESPEED;
        }

        if (max_movespeed > ABSOLUTE_MAX_MOVESPEED)
        {
            max_movespeed = ABSOLUTE_MAX_MOVESPEED;
        }

        last_movespeed = cur_ms;

        if (!alive)
        {
            return;
        }


        float speed;
        GetInput(out speed);

        Vector3 desiredMove = m_Camera.transform.forward * m_Input.y + m_Camera.transform.right * m_Input.x;

        m_MoveDir.x = desiredMove.x * MOVESPEED;

        if (touchingDoor)
        {
            if (desiredMove.normalized.z > 0.75)
            {
                main.triggerNext();
            }
        }
        
        if (onLadder && ladder_detach_timeout < Time.time)
        {
            float look_y = m_Camera.transform.forward.y;
            Vector3 vector_xz = new Vector3(m_Camera.transform.forward.x, 0f, m_Camera.transform.forward.z);
            float look_xz = vector_xz.magnitude;
            m_MoveDir.y = desiredMove.y * CLIMBSPEED;
            if (Math.Abs(look_y) > look_xz)
            {
                rb.velocity = new Vector3(0f, rb.velocity.y, rb.velocity.z);
                
                if (look_y < 0)
                {
                }
                else
                {
                }

            }
        }
        else
        {
            m_MoveDir.y = 0.0f;
        }

        if (m_Jump && (isGrounded || onLadder || steppedOnTimer > Time.time))
        {
            Force(0, JUMPFORCE);
            m_Jump = false;
            if (onLadder)
            {
                ladder_detach_timeout = Time.time + 0.2f;
            }
            PlayFootStepAudio();
        }

        Vector3 force = m_MoveDir * MOVEFORCE;
        Force(force.x, force.y);

        if (Mathf.Abs(rb.velocity.x) > max_movespeed)
        {
            rb.velocity = new Vector3(Mathf.Sign(rb.velocity.x) * max_movespeed, rb.velocity.y, rb.velocity.z);
        }

        if (onLadder && Mathf.Abs(rb.velocity.y) > MAX_CLIMBSPEED && ladder_detach_timeout < Time.time)
        {
            rb.velocity = new Vector3(rb.velocity.x, Mathf.Sign(rb.velocity.y) * MAX_CLIMBSPEED, rb.velocity.z);
        }
        
        if (rb.velocity.y > MAX_JUMPSPEED)
        {
            rb.velocity = new Vector3(rb.velocity.x, MAX_JUMPSPEED, rb.velocity.z);
        }

        if (onLadder && ladder_detach_timeout < Time.time)
        {
            rb.useGravity = false;
            rb.drag = 20;
        }
        else
        {
            rb.useGravity = true;
            rb.drag = 0;
        }

        ProgressStepCycle();
        UpdateCameraPosition(speed);

        m_MouseLook.UpdateCursorLock();

        velocities.Insert(0, new Vector3(rb.velocity.x, rb.velocity.y, rb.velocity.z));
        if (velocities.Count > 10)
        {
            velocities.RemoveAt(velocities.Count - 1);
        }
    }

    int levelGeomLayerMask = 1 << 9;

    void Force(float x, float y)
    {
        if (x != 0)
        {
            var force = new Vector3(x, 0, 0);
            var sign = x < 0 ? -1 : 1;
            if (!(Physics.Raycast(transform.position + new Vector3(sign * 0.49f, 0.0f), sign * Vector3.right, MOVE_CHECK_DIST, levelGeomLayerMask)
                || Physics.Raycast(transform.position + new Vector3(sign * 0.49f, 0.5f), sign * Vector3.right, MOVE_CHECK_DIST, levelGeomLayerMask)
                || Physics.Raycast(transform.position + new Vector3(sign * 0.49f, 1.0f), sign * Vector3.right, MOVE_CHECK_DIST, levelGeomLayerMask)
                || Physics.Raycast(transform.position + new Vector3(sign * 0.49f, 1.5f), sign * Vector3.right, MOVE_CHECK_DIST, levelGeomLayerMask)
                || Physics.Raycast(transform.position + new Vector3(sign * 0.49f, 2.0f), sign * Vector3.right, MOVE_CHECK_DIST, levelGeomLayerMask)))
            {
                rb.AddForce(force);
            }
        }
        if (y != 0)
        {
            var force = new Vector3(0, y, 0);
            var sign = y < 0 ? -1 : 1;
            if (!(Physics.Raycast(transform.position + new Vector3(-0.5f, 1.0f + sign * 0.99f), sign * Vector3.up, MOVE_CHECK_DIST, levelGeomLayerMask)
                || Physics.Raycast(transform.position + new Vector3(0f, 1.0f + sign * 0.99f), sign * Vector3.up, MOVE_CHECK_DIST, levelGeomLayerMask)
                || Physics.Raycast(transform.position + new Vector3(0.5f, 1.0f + sign * 0.99f), sign * Vector3.up, MOVE_CHECK_DIST, levelGeomLayerMask)))
            {
                rb.AddForce(force);
            }
        }
    }

    float jumpLastPlayed = 0;
    
    private void PlayJumpSound()
    {
        if (Time.time - jumpLastPlayed > 0.1f)
        {
            m_AudioSource.clip = m_JumpSound;
            m_AudioSource.Play();
            jumpLastPlayed = Time.time;
        }
    }


    private void ProgressStepCycle()
    {
        
        if (Mathf.Abs(rb.velocity.x) < 0.2f || !isGrounded)
        {
            return;
        }

        if (Time.time < m_NextStep)
        {
            return;
        }

        m_NextStep = Time.time + 0.4f;

        PlayFootStepAudio();
    }

    int audio = 0;

    private void PlayFootStepAudio()
    {
        if (Time.time - jumpLastPlayed > 0.1f)
        {
            m_AudioSource.clip = m_FootstepSounds[audio];
            m_AudioSource.Play();
            jumpLastPlayed = Time.time;

            audio++;
            if (audio >= m_FootstepSounds.Length)
            {
                audio = 0;
            }
        }
    }


    private void UpdateCameraPosition(float speed)
    {
        Vector3 newCameraPosition;
        if (!m_UseHeadBob)
        {
            return;
        }
        if (rb.velocity.magnitude > 0 && isGrounded)
        {
            m_Camera.transform.localPosition =
                m_HeadBob.DoHeadBob(rb.velocity.magnitude + speed);
            newCameraPosition = m_Camera.transform.localPosition;
            newCameraPosition.y = m_Camera.transform.localPosition.y - m_JumpBob.Offset();
        }
        else
        {
            newCameraPosition = m_Camera.transform.localPosition;
            newCameraPosition.y = m_OriginalCameraPosition.y - m_JumpBob.Offset();
        }
        m_Camera.transform.localPosition = newCameraPosition;
    }


    private void GetInput(out float speed)
    {
        // Read input
        float horizontal = CrossPlatformInputManager.GetAxis("Horizontal");
        float vertical = CrossPlatformInputManager.GetAxis("Vertical");

        // set the desired speed to be walking or running
        speed = MOVESPEED;
        m_Input = new Vector2(horizontal, vertical);

        // normalize input if it exceeds 1 in combined length:
        if (m_Input.sqrMagnitude > 1)
        {
            m_Input.Normalize();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        Enemy enemy = collision.gameObject.GetComponent<Enemy>();

        if (enemy != null)
        {
            float enemyTop = enemy.transform.position.y + enemy.height;
            //if (collision.relativeVelocity.y > 0 && enemyTop < transform.position.y + 0.1f)
            if (getAvgYVelocity() < 0 && !isGrounded)
            {
                enemy.Die();
                steppedOnTimer = Time.time + 0.1f;
                //max_movespeed += MOVESPEED_INCREMENT;
                max_movespeed *= 1.5f;
            }
            else
            {
                Die();
            }
        }

    }

    bool touchingDoor = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 8)
        {
            ladderCount++;
            onLadder = true;
        }
        else if (other.gameObject.layer == 10)
        {
            touchingDoor = true;
        }
        else
        {
            Die();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 8)
        {
            ladderCount--;
            if (ladderCount <= 0)
            {
                onLadder = false;
            }
        }
        if (other.gameObject.layer == 10)
        {
            touchingDoor = false;
        }
    }

    public void Die()
    {
        if (alive)
        {
            GameObject obj = Instantiate(deathSplat);
            obj.transform.parent = transform.parent;
            obj.transform.position = transform.position + new Vector3(0, 1.5f, 0) + m_Camera.transform.forward * 0.5f;
            Destroy(obj, 20.0f);

            rb.constraints = RigidbodyConstraints.None;
            Vector3 direction = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(0.5f, 1.0f), (Random.Range(0, 10.0f) < 5.0f ? -1 : 1) * Random.Range(0.8f, 1.0f));
            rb.AddRelativeForce(direction * 200f);

            rb.useGravity = true;
            rb.drag = 0;

            alive = false;

            GetComponent<Collider>().enabled = false;

            main.triggerDeath();
        }
    }
}

