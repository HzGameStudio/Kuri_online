using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedBoostScript : MonoBehaviour
{
    // Start is called before the first frame update
    public SpeedBoostScriptableObject speedBoostData;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Debug.Log(collision.gameObject.tag);
        if (collision.gameObject.CompareTag("player"))
        {

            collision.gameObject.GetComponent<PlayerControl>().isSpeedBoosted = true;
            collision.gameObject.GetComponent<PlayerControl>().curSpeedBoostTime = speedBoostData.boostTime;
            collision.gameObject.GetComponent<PlayerControl>().curSpeedBoostForce = speedBoostData.boostForce;
        }
    }
    
}
