using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;

public class gameData : NetworkBehaviour
{
    public NetworkVariable<int> numPlayersInGame = new NetworkVariable<int>();
    public bool isGameRuning = true;

    public int numFinishedPlayers = 0;
    public void CalcNumPlayersInGame()
    {
        GameObject[] playerList = GameObject.FindGameObjectsWithTag("player");
        numPlayersInGame.Value = playerList.Length;
    }
}
 