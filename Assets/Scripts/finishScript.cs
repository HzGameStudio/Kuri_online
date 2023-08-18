using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class FinishScript : NetworkBehaviour
{
    private GameData gameManagerGameData;

    void Start()
    {
        gameManagerGameData = GameObject.FindGameObjectWithTag("gameManager").GetComponent<GameData>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsServer)
        {
            Debug.Log(collision.gameObject.tag);
            if (collision.gameObject.CompareTag("player"))
            {
                //end of game someone won
                Debug.Log("finish of the game hz wich kura won");
                gameManagerGameData.numFinishedPlayers++;
                collision.gameObject.GetComponent<PlayerData>().placeInGame.Value = gameManagerGameData.numFinishedPlayers;
                collision.gameObject.GetComponent<PlayerData>().finishedgame.Value = true;
                //gameManagerGameData.isGameRunning = false;
            }
        }
    }
}
