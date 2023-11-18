using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointScript : MonoBehaviour
{
    public Transform spawnPosition;
    public Vector3 velocity = Vector3.zero; 
    public float gravityM = 2;
    public int gravityD = 1;

    private bool isActive = false;

    public SpriteRenderer textureSprite;

    //the spawn data shpuld be added

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision == null) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            if(collision.gameObject.GetComponent<PlayerMain>().SetCheckPoint(spawnPosition.position, velocity, gravityM, gravityD))
            {
                isActive = true;
                textureSprite.color = Color.green;
            }
        }
    }
}
