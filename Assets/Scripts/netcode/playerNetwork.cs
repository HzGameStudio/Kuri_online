using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class playerNetwork : NetworkBehaviour
{
    private readonly NetworkVariable<Vector3> position = new NetworkVariable<Vector3>(writePerm: NetworkVariableWritePermission.Owner);
    private readonly NetworkVariable<Vector2> velocity  = new NetworkVariable<Vector2>(writePerm: NetworkVariableWritePermission.Owner);
    private readonly NetworkVariable<float> m_GravityDirection = new NetworkVariable<float>(writePerm: NetworkVariableWritePermission.Owner);
    // Update is called once per frame
    void Update()
    {
        if(IsOwner)
        {
            position.Value = gameObject.transform.position;
           //m_GravityDirection.Value = gameObject.GetComponent<Rigidbody2D>().gravityScale;
           // velocity.Value = gameObject.GetComponent<Rigidbody2D>().velocity;
        }
        else
        {
            gameObject.transform.position = position.Value;
            //gameObject.GetComponent<Rigidbody2D>().gravityScale = m_GravityDirection.Value;
            //gameObject.GetComponent<Rigidbody2D>().velocity = velocity.Value;
        }
    }
}
