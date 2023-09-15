using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class FinishScript : NetworkBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsServer)
        {
            if (collision.gameObject.CompareTag("player"))
            {
                MainManager.Instance.numFinishedPlayers++;

                PlayerData playerData = collision.gameObject.GetComponent<PlayerData>();
                playerData.placeInGame.Value = MainManager.Instance.numFinishedPlayers;
                playerData.finishedGame.Value = true;
            }
        }
    }
}
