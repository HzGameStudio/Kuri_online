using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointScript : MonoBehaviour
{
    public Transform spawnPosition;

    private bool isActive = false;

    public SpriteRenderer textureSprite;

    //the spawn data shpuld be added

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision == null) return;

        if (collision.gameObject.CompareTag("player"))
        {
            if(collision.gameObject.GetComponent<PlayerMain>().SetCheckPoint(spawnPosition.position))
            {
                isActive = true;
                textureSprite.color = Color.green;
            }
        }
    }
}
