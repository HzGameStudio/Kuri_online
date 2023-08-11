using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerData : NetworkBehaviour
{
    public NetworkVariable<bool> FinishedGame = new NetworkVariable<bool>(false);
    public NetworkVariable<int> playerID = new NetworkVariable<int>(-1);
    public NetworkVariable<int> placeInGame = new NetworkVariable<int>(-1);

    private GameData gameManagerGameData;

    private void Start()
    {
        gameManagerGameData = GameObject.FindGameObjectWithTag("gameManager").GetComponent<GameData>();

        playerID.Value = gameManagerGameData.numPlayersInGame.Value;
    }
}
