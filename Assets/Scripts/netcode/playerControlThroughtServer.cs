using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class playerControlThroughtServer : NetworkBehaviour
{
    [SerializeField]

    private float walkSpeed = 3.5f;

    [SerializeField]
    private Vector2 defaultPositionRange = new Vector2(-4, 4);

    [SerializeField]
    private NetworkVariable<float> forwardBackPosition = new NetworkVariable<float>();

    [SerializeField]
    private NetworkVariable<float> leftRightPosition = new NetworkVariable<float>();

    //client caching

    private float oldForvardBackPosition;
    private float oldLeftRightPosition;

    private void Start()
    {
        transform.position = new Vector3(Random.Range(defaultPositionRange.x, defaultPositionRange.y), 0, Random.Range(defaultPositionRange.x, defaultPositionRange.y));

    }

    private void UpdateServer()
    {
        transform.position = new Vector3(transform.position.x + leftRightPosition.Value, transform.position.y, transform.position.z + forwardBackPosition.Value);

    }

    private void UpdateClient()
    {
        float forwarBackward = 0;
        float leftRight = 0;

        if(Input.GetKey(KeyCode.W))
        {
            forwarBackward = walkSpeed;
        }

        if (Input.GetKey(KeyCode.S))
        {
            forwarBackward = -walkSpeed;
        }

        if (Input.GetKey(KeyCode.A))
        {
            leftRight = walkSpeed;
        }

        if (Input.GetKey(KeyCode.D))
        {
            leftRight = -walkSpeed;
           
        }

        if(oldForvardBackPosition != forwarBackward || oldLeftRightPosition != leftRight)
        {
            oldForvardBackPosition = forwarBackward;
            oldLeftRightPosition = leftRight;
            //update server
            Debug.Log("update");
            UpdateClientPositionServerRpsServerRpc(forwarBackward, leftRight);
        }
    }

    [ServerRpc]

    public void UpdateClientPositionServerRpsServerRpc(float forwarBackward, float leftRight)
    {
        forwardBackPosition.Value = forwarBackward;
        leftRightPosition.Value = leftRight;

    }

    private void Update()
    {
        if(IsServer)
        {
            UpdateServer();
        }

        if(IsClient && IsOwner)
        {
            UpdateClient();
        }
    }



}
