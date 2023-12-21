using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SpeedBoostScript : MonoBehaviour
{
    public SpeedBoostScriptableObject speedBoostData;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Debug.Log(collision.gameObject.tag);
        if (collision.gameObject.CompareTag("player"))
        {
            collision.gameObject.GetComponent<IPlayerMain>().Boost(speedBoostData);
        }
    }
    
}
