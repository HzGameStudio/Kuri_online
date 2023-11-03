using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class FinishScript : NetworkBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("player"))
        {
            MainManager.Instance.numFinishedPlayers++;

            collision.gameObject.GetComponent<PlayerMain>().Finish();

            if (IsServer)
            {
                if (MainManager.Instance.numFinishedPlayers == MainManager.Instance.numPlayersInGame.Value)
                    MainManager.Instance.sceneObjectsCache.restartButton.SetActive(true);
            }
        }
    }
}
