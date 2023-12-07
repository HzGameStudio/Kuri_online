using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointScript : MonoBehaviour
{
    public Transform spawnPosition;
    [HideInInspector]
    public Vector3 velocity = Vector3.zero; 
    [HideInInspector]
    public float gravityM = 2;
    [HideInInspector]
    public int gravityD = 1;
    [HideInInspector]
    public int nFlips = 1;

    private bool isActive = false;

    public SpriteRenderer textureSprite;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision == null) return;

        if (collision.gameObject.CompareTag("player"))
        {
            if(collision.gameObject.GetComponent<PlayerMain>().SetCheckPoint(new PlayerMovementManager.KuraTransfromData(spawnPosition.position, velocity, gravityD, gravityM, nFlips)))
            {
                isActive = true;
                textureSprite.color = Color.green;
            }
        }
    }
}
