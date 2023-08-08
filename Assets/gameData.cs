using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;

public class gameData : MonoBehaviour
{
    public int numPlayersInGame;
    public bool isGameRuning = true;

    public int numFinishedPlayers = 0;
    public void CalcNumPlayersInGame()
    {
        GameObject[] playerList = GameObject.FindGameObjectsWithTag("player");
        numPlayersInGame = playerList.Length;
    }
}
 