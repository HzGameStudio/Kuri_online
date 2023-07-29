using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TestPlayerControl : MonoBehaviour
{
    [SerializeField] private LayerMask platformLayerMask;

    //cringe code veriables section:)
    private bool enteredFlipCollider = false;

    public Camera m_Camera;
    public float onGroundVelocity = 5f;
    public float brakeVelocity = 0.005f;
    public float maxVelocity = 10f;

    private float timeOfAcselerationOfPlatform = 1f;
    private float currentAcseleration;

    BoxCollider2D m_boxCollider2D;
    Rigidbody2D m_rigidBody2d;


    int numberOfGravityChangeAvailable = 1;
    int maxNumberOfGravityChangeAvailable = 1;
    void Start()
    {
        m_rigidBody2d = GetComponent<Rigidbody2D>();
        m_rigidBody2d.freezeRotation = true;
        m_boxCollider2D = gameObject.GetComponent<BoxCollider2D>();
    }

    bool m_GravityDirection = false;

    public int force = 1;



    void Update()
    {
        if ((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) || Input.GetMouseButtonDown(0))
        {
            if(numberOfGravityChangeAvailable > 0)
            {
                m_GravityDirection = !m_GravityDirection;
                gameObject.GetComponent<Rigidbody2D>().gravityScale = m_GravityDirection ? 1 : -1;
                numberOfGravityChangeAvailable -= 1;
                numberOfGravityChangeAvailable = (numberOfGravityChangeAvailable < 0) ? 0 : numberOfGravityChangeAvailable;
                //m_view.RPC("SetGravity", RpcTarget.All, m_GravityDirection);
            }

        }



        m_Camera.GetComponent<Transform>().position = gameObject.transform.position + new Vector3(0, 0, -10);
    }

    private void FixedUpdate()
    {
        Debug.Log(currentAcseleration);
        if (checkGround())
        {
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
        else
        {
            m_rigidBody2d.AddForce(Vector2.right * force);
        }

        //Debug.Log(numberOfGravityChangeAvailable);
        //Debug.Log(enteredFlipCollider);
        //if(checkForAdditionalFlip())
        //{
        //    numberOfGravityChangeAvailable += 1;
        //    numberOfGravityChangeAvailable = (numberOfGravityChangeAvailable > 1) ? 1 : numberOfGravityChangeAvailable;
        //}

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

    private bool checkForAdditionalFlip()
    {

        bool result = false;
        float extraBoxHeight = 0.1f;
        string[] flipTagList = { "simplePlatform" };
        RaycastHit2D[] raycasthit = Physics2D.BoxCastAll(m_boxCollider2D.bounds.center, new Vector2(m_boxCollider2D.bounds.size.x + extraBoxHeight, m_boxCollider2D.bounds.size.y + extraBoxHeight), 0f, Vector2.zero, 0f);
        for (int i = 0; i < raycasthit.Length; i++)
        {
            if (raycasthit[i].collider != null && findElement(flipTagList, raycasthit[i].collider.tag))
            {
                
                result= true;
                
            }
        }
        if(result && !enteredFlipCollider)
        {
            enteredFlipCollider = true;
            return true;
        }
        else if(!result && enteredFlipCollider)
        {
            enteredFlipCollider = false;
            
        }

        return false;


    }

    private bool findElement(string[] arrey, string element)
    {
        for(int i=0; i<arrey.Length; i++)
        {
            if (element == arrey[i]) return true;
        }
        return false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        string[] flipTagList = { "simplePlatform" };
        if(findElement(flipTagList, collision.gameObject.tag))
        {
            numberOfGravityChangeAvailable += 1;
            numberOfGravityChangeAvailable = (numberOfGravityChangeAvailable > maxNumberOfGravityChangeAvailable) ? maxNumberOfGravityChangeAvailable : numberOfGravityChangeAvailable;
            
            
            
        }
        currentAcseleration = Mathf.Abs(m_rigidBody2d.velocity.magnitude - onGroundVelocity) / 50f;
    }
        
}

