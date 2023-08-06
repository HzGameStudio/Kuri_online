using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class playerNetworkTutorial : NetworkBehaviour
{
    
    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;
        Vector3 MoveDirection = new Vector3(0, 0, 0);
        if (Input.GetKey(KeyCode.W)) MoveDirection.z = 1f;
        if (Input.GetKey(KeyCode.S)) MoveDirection.z = -1f;
        if (Input.GetKey(KeyCode.A)) MoveDirection.x = -1f;
        if (Input.GetKey(KeyCode.D)) MoveDirection.x = 1f;

        float moveSpeep = 3f;

        transform.position += MoveDirection * moveSpeep * Time.deltaTime;



    }
}
