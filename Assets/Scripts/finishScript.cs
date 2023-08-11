using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class FinishScript : NetworkBehaviour
{
    [SerializeField]
    private GameData gameManagerGameData;

    void Start()
    {
        Debug.Log(GameObject.FindGameObjectWithTag("gameManager"));
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
                collision.gameObject.GetComponent<PlayerUIManager>().placeInGame.Value = gameManagerGameData.numFinishedPlayers;
                //gameManagerGameData.isGameRunning = false;
            }
        }
    }
}
