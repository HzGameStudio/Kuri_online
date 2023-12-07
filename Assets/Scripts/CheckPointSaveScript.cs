using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointSaveScript : MonoBehaviour
{
    private bool isActive = false;

    public SpriteRenderer textureSprite;


    //the spawn data shpuld be added

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision == null) return;

        if (collision.gameObject.CompareTag("player"))
        {
            if(collision.gameObject.GetComponent<PlayerMain>().SetCheckPoint())
            {
                isActive = true;
                textureSprite.color = Color.green;
            }
        }
    }
}
