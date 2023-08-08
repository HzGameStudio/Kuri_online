using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class finishScript : MonoBehaviour
{
    [SerializeField]
    private gameData gameManagerGameData;

    void Start()
    {
        gameManagerGameData = GameObject.FindGameObjectWithTag("gameManager").GetComponent<gameData>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(collision.gameObject.tag);
        if(collision.gameObject.CompareTag("player"))
        {
            //end of game someone won
            Debug.Log("finish of the game hz wich kura won");
            gameManagerGameData.numFinishedPlayers++;
            collision.gameObject.GetComponent<PlayerControl>().placeInGame = gameManagerGameData.numFinishedPlayers;
            //gameManagerGameData.isGameRuning = false;
        }
    }


}
