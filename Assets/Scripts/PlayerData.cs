using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

// Class that stores general data about the player (see below), it is also the only class that other classes can take data from
// <PlayerUIManager> and <PlayerControl> should not be accessed by other classes !!
public class PlayerData : NetworkBehaviour
{
    // The states that a kura can be in,
    // used for animation and physics (and a lot of other stuff TBA)
    // The state of a kura generally depends on:
    // 1) Whether the kura is on the ground
    // 2) Whether the kura is bumped into a wall and can't (shouldn't) move forward
    // 3) Speed of the kura
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

    public enum GameState
    {
        //normal game mode
        ClasicMode,
        // mane speak for it self :)
        SpactatorMode
    }

    ///SpactatorMode
    public int currentSpactatorModeIndex;


    public const float playerStartHealhtConst = 100;
    public float playerStartHealht = playerStartHealhtConst;

    public NetworkVariable<bool> finishedgame = new NetworkVariable<bool>(false);
    public NetworkVariable<int> playerID = new NetworkVariable<int>(-1);
    public NetworkVariable<int> placeInGame = new NetworkVariable<int>(-1);
    public NetworkVariable<float> playerRunTime = new NetworkVariable<float>(0);
    public NetworkVariable<KuraState> state = new NetworkVariable<KuraState>(KuraState.Fall);
    public NetworkVariable<GameState> gameState = new NetworkVariable<GameState>(GameState.ClasicMode);
    public NetworkVariable<Vector3> spawnPosition = new NetworkVariable<Vector3>();
    public NetworkVariable<float> playerHealht = new NetworkVariable<float>(playerStartHealhtConst);

    private GameData m_GameData;

    private void Start()
    {
        // <GameManager>.<GameData> is the only we need to find when the player spawns in,
        // it usually has all the player needs in the scene, to minimize GameObject.Find() calls, as they are costly
        m_GameData = GameObject.FindGameObjectWithTag("gameManager").GetComponent<GameData>();
        if (IsServer)
        {
            m_GameData.CalcNumPlayersInGame();
            playerID.Value = m_GameData.numPlayersInGame.Value;
        }
    }
}
