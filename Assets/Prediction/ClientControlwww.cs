using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Scripting.APIUpdating;

public class ClientControl : NetworkBehaviour
{
    [SerializeField] GameObject Camera;

    private Rigidbody2D rb;
    public override void OnNetworkSpawn()
    {
        if(!IsOwner)
        {
            Camera.SetActive(false);
        }
        base.OnNetworkSpawn();
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        if(IsOwner)
        {
            transform.position =new Vector3(transform.position.x + Random.Range(5, 10), transform.position.y, transform.position.z);

            var relay = GameObject.FindObjectOfType<PredictionRelay>();
            relay.joinButton.SetActive(false);
            relay.createButton.SetActive(false);
            relay.joinfield.SetActive(false);
        }
    }

    void Update()
    {
        
        if(IsOwner)
        {
            Move(GetInput());
        }
    }

    void Move(Vector2 input)
    {
        float acseleration = 9;
        rb.AddForce(input * 5f);
    }

    Vector2 GetInput()
    {
        Vector2 input = new();
        if(Input.GetKey(KeyCode.W))
        {
            input.y = 1;
        }
        else if(Input.GetKey(KeyCode.S))
        {
            input.y = -1;
        }
        input.x = 0;
        return input;
    }
}
