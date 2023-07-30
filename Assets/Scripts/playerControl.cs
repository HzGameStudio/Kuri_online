using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

public class playerControl : MonoBehaviour
{
    public enum KuraState
    {
        //No speed, air
        Fall,
        //No\Normal speed, ground
        Run,
        //Too much speed, ground
        FlapRun,
        //Normal speed, air
        Fly,
        //Too much speed, air
        Glide
    }

    // *** Constants

    // Objects

    public Camera m_Camera;
    BoxCollider2D m_BoxCollider2D;
    Rigidbody2D m_RigidBody2d;
    PhotonView m_View;
    public Transform m_Transform;

    // Physics

    public const float m_OnGroundVelocity = 5f;
    public const float m_BrakeVelocity = 0.005f;
    public const float m_MaxVelocity = 10f;
    public const int force = 1;
    public const float m_TimeOfAcselerationOfPlatform = 1f;

    // Logic

    string[] m_FlipTagList = { "simplePlatform" };

    const int m_MaxNumberOfGravityChangeAvailable = 1;



    // *** Active

    // 0 = -1, 1 = 1
    bool m_GravityDirection = false;

    float m_CurrentAcseleration;

    int m_NumberOfGravityChangeAvailable = 1;

    public KuraState m_State = KuraState.Fall;



    //void SetVelocity(Vector2 velocity)
    //{
    //    gameObject.GetComponent<Rigidbody2D>().velocity = velocity;
    //}

    // Start is called before the first frame update
    void Start()
    {
        m_RigidBody2d = GetComponent<Rigidbody2D>();
        m_BoxCollider2D = gameObject.GetComponent<BoxCollider2D>();
        m_View = GetComponent<PhotonView>();

        m_RigidBody2d.freezeRotation = true;

        if (m_View.IsMine)
        {
            if (m_Camera.gameObject.activeInHierarchy == false)
            {
                m_Camera.gameObject.SetActive(true);
            }
        }
    }

    [PunRPC]
    void SetGravity(bool GravityDirection)
    {
        m_GravityDirection = GravityDirection;
        gameObject.GetComponent<Rigidbody2D>().gravityScale = m_GravityDirection ? 1 : -1;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_View.IsMine)  
        {
            if ((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) || Input.GetMouseButtonDown(0))
            {
                if(m_NumberOfGravityChangeAvailable > 0)
                {
                    m_GravityDirection = !m_GravityDirection;
                    m_RigidBody2d.gravityScale = m_GravityDirection ? 1 : -1;
                    m_View.RPC("SetGravity", RpcTarget.All, m_GravityDirection);
                    m_Transform.localScale = new Vector3(m_Transform.localScale.x, m_Transform.localScale.y * -1, m_Transform.localScale.z);
                    m_NumberOfGravityChangeAvailable--;
                }
                
            }

            m_Camera.GetComponent<Transform>().position = gameObject.transform.position + new Vector3(0, 0, -10);
        }
        //Debug.Log(m_NumberOfGravityChangeAvailable);
    }

    private bool checkGround()
    {
        float extraBoxHeight = 0.1f;
        RaycastHit2D[] raycasthit = Physics2D.BoxCastAll(m_BoxCollider2D.bounds.center, new Vector2(m_BoxCollider2D.bounds.size.y, m_BoxCollider2D.bounds.size.y + extraBoxHeight), 0f, Vector2.zero, 0f);

        for (int i = 0; i < raycasthit.Length; i++)
        {
            //Debug.Log(raycasthit[i].collider.tag);
            if (raycasthit[i].collider != null && raycasthit[i].collider.CompareTag("simplePlatform"))
            {
                //Debug.Log(raycasthit[i].collider.tag);
                return true;
            }
        }
        return false;
    }

    private void FixedUpdate()
    {
        if (m_View.IsMine)
        {
            if (checkGround())
            {
                if (m_RigidBody2d.velocity.magnitude > m_OnGroundVelocity)
                {
                    m_RigidBody2d.velocity -= Vector2.right * m_CurrentAcseleration;
                }
                else
                {
                    if (m_RigidBody2d.velocity.magnitude < m_MaxVelocity)
                    {
                        m_RigidBody2d.velocity += Vector2.right * m_CurrentAcseleration;
                    }
                    else
                    {
                        m_RigidBody2d.velocity = Vector2.right * m_OnGroundVelocity;
                    }
                }

            }
            else
            {
                m_RigidBody2d.AddForce(Vector2.right * force);
            }
            //m_View.RPC("SetVelocity", RpcTarget.All, m_RigidBody2d.velocity);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Debug.Log(collision.gameObject.tag);
        if (Array.Exists(m_FlipTagList, element => element == collision.gameObject.tag))
        {
            m_NumberOfGravityChangeAvailable += 1;
            m_NumberOfGravityChangeAvailable = Math.Min(m_NumberOfGravityChangeAvailable, m_MaxNumberOfGravityChangeAvailable);
        }
        m_CurrentAcseleration = Mathf.Abs(m_RigidBody2d.velocity.magnitude - m_OnGroundVelocity) / 50f;

        /*
        foreach (ContactPoint2D contact in collision.contacts)
        {
            Debug.DrawLine(new Vector3(contact.point.x, contact.point.y, transform.position.z), transform.position, Color.red, 2, false);
        }
        Debug.Break();
        */
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        
    }
}
