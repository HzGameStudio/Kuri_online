using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;
public class playerContrilNewMethodf : NetworkBehaviour
{
    //server veriable

    private NetworkVariable<bool> m_request  = new NetworkVariable<bool>(false);

    //private NetworkVariable<float> m_numberOfFlips = new NetworkVariable<float>(1);

    private float m_numberOfFlips = 1;
    private float m_gravityDiretion = 1;

    private float currentAcseleration;

    [SerializeField]
    private int maxNumberOfGravityChangeAvailable = 1;

    [SerializeField]
    private float maxVelocity = 10f;



    [SerializeField]
    private float onGroundVelocity= 2f;

    //client veriablse;
    private int oldGravityDirection = 1;

    private float oldNumberOfFlips = 1;

    [SerializeField]
    private Camera m_Camera;

    [SerializeField]
    private BoxCollider2D m_boxCollider2D;

    [SerializeField]
    private Rigidbody2D m_rigidBody2d;


    // Update is called once per frame
    void Update()
    {
        if (IsServer)
        {
            UpdateServer();
        }

        if (IsClient && IsOwner)
        {
            UpdateClient();
        }
    }

    private void UpdateServer()
    {
        if(m_request.Value)
        {
            if(m_numberOfFlips > 0)
            {
                m_request.Value = false;
                m_gravityDiretion *= -1;
                gameObject.GetComponent<Rigidbody2D>().gravityScale = m_gravityDiretion;

                m_numberOfFlips -= 1;
                m_numberOfFlips = (m_numberOfFlips < 0) ? 0 : m_numberOfFlips;
            }else
            {
                m_request.Value = false;
            }
            
        }
        

        if (checkGround())
        {
            gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.right * onGroundVelocity;
            if (m_rigidBody2d.velocity.magnitude > onGroundVelocity)
            {
                m_rigidBody2d.velocity -= Vector2.right * currentAcseleration;
            }
            else
            {
                if (m_rigidBody2d.velocity.magnitude < maxVelocity)
                {
                    m_rigidBody2d.velocity += Vector2.right * currentAcseleration;
                }
                else
                {
                    m_rigidBody2d.velocity = Vector2.right * onGroundVelocity;
                }
            }
        }
    }

    private bool checkGround()
    {
        float extraBoxHeight = 0.1f;
        RaycastHit2D[] raycasthit = Physics2D.BoxCastAll(m_boxCollider2D.bounds.center, new Vector2(m_boxCollider2D.bounds.size.x, m_boxCollider2D.bounds.size.y + extraBoxHeight), 0f, Vector2.zero, 0f);

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

    private void UpdateClient()
    {
        if (IsOwner && !m_Camera.gameObject.activeInHierarchy)
        {
            m_Camera.gameObject.SetActive(true);
        }
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            oldGravityDirection *= -1;
            UpdateClientPositionServerRpsServerRpc(true);
        }
    }

    [ServerRpc]
    public void UpdateClientPositionServerRpsServerRpc(bool request)
    {
        m_request.Value = request;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsServer) return;
        
        string[] flipTagList = { "simplePlatform", "player" };
        if (findElement(flipTagList, collision.gameObject.tag))
        {
            m_numberOfFlips += 1;
            m_numberOfFlips = (m_numberOfFlips > m_numberOfFlips) ? maxNumberOfGravityChangeAvailable : m_numberOfFlips;
        
        
        
        }
        currentAcseleration = Mathf.Abs(m_rigidBody2d.velocity.magnitude - onGroundVelocity) / 50f;
        

    }

    private bool findElement(string[] arrey, string element)
    {
        for (int i = 0; i < arrey.Length; i++)
        {
            if (element == arrey[i]) return true;
        }
        return false;
    }
}
