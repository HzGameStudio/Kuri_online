using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerData : NetworkBehaviour
{
    public NetworkVariable<bool> FinishedGame = new NetworkVariable<bool>(false);
    public NetworkVariable<int> playerID = new NetworkVariable<int>(-1);
    public NetworkVariable<int> placeInGame = new NetworkVariable<int>(-1);
    public NetworkVariable<float> playerRunTime = new NetworkVariable<float>(0);

    private GameData m_GameManagerGameData;

    private void Start()
    {
        m_GameManagerGameData = GameObject.FindGameObjectWithTag("gameManager").GetComponent<GameData>();
        if (IsServer)
        {
            m_GameManagerGameData.CalcNumPlayersInGame();
            playerID.Value = m_GameManagerGameData.numPlayersInGame.Value;
        }
    }
}
