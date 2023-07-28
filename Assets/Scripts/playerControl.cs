using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class playerControl : MonoBehaviour
{
    //0 = -1, 1 = 1
    bool m_GravityDirection = false;

    PhotonView m_view;

    [PunRPC]
    void SetGravity(bool GravityDirection)
    {
        m_GravityDirection = GravityDirection;
        gameObject.GetComponent<Rigidbody2D>().gravityScale = m_GravityDirection ? 1 : -1;
    }

    // Start is called before the first frame update
    void Start()
    {
        m_view = GetComponent<PhotonView>();
        GetComponent<Rigidbody2D>().freezeRotation = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_view.IsMine)  
        {
            if ((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) || Input.GetMouseButtonDown(0))
            {
                m_GravityDirection = !m_GravityDirection;
                gameObject.GetComponent<Rigidbody2D>().gravityScale = m_GravityDirection ? 1 : -1;
                m_view.RPC("SetGravity", RpcTarget.All, m_GravityDirection);
            }
        }
    }
}
