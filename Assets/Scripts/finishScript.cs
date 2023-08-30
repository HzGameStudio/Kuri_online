using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class FinishScript : NetworkBehaviour
{
    private GameData m_GameData;

    void Start()
    {
        m_GameData = GameObject.FindGameObjectWithTag("gameManager").GetComponent<GameData>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsServer)
        {
            if (collision.gameObject.CompareTag("player"))
            {
                m_GameData.numFinishedPlayers++;
                collision.gameObject.GetComponent<PlayerData>().placeInGame.Value = m_GameData.numFinishedPlayers;
                collision.gameObject.GetComponent<PlayerData>().finishedgame.Value = true;
            }
        }
    }
}
