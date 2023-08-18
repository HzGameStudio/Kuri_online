using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class PlayerData : NetworkBehaviour
{
    public enum KuraState
    {
        //Kissing a wall, ground
        Stand,
        //No speed, air
        Fall,
        //No speed, ground
        Run,
        //Normal speed, ground
        ReadyRun,
        //Too much speed, ground
        FlapRun,
        //Normal speed, air
        Fly,
        //Too much speed, air
        Glide
    }

    public NetworkVariable<bool> finishedgame = new NetworkVariable<bool>(false);
    public NetworkVariable<int> playerID = new NetworkVariable<int>(-1);
    public NetworkVariable<int> placeInGame = new NetworkVariable<int>(-1);
    public NetworkVariable<float> playerRunTime = new NetworkVariable<float>(0);
    public NetworkVariable<KuraState> state = new NetworkVariable<KuraState>(KuraState.Fall);

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
