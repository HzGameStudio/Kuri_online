using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class finishScript : NetworkBehaviour
{
    [SerializeField]
    private gameData gameManagerGameData;

    void Start()
    {
        Debug.Log(GameObject.FindGameObjectWithTag("gameManager"));
        gameManagerGameData = GameObject.FindGameObjectWithTag("gameManager").GetComponent<gameData>();
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
                collision.gameObject.GetComponent<PlayerControl>().placeInGame.Value = gameManagerGameData.numFinishedPlayers;
                //gameManagerGameData.isGameRuning = false;
            }
        }
    }
}
