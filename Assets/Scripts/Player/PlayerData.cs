using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

// Class to stores general data about the player, it is also the only class that other classes can take data from
// <PlayerUIManager> and <PlayerControl> should not be accessed by other classes
public class PlayerData : NetworkBehaviour
{
    // The states that a kura can be in,
    // used for animation and physics
    // The state of a kura generally depends on:
    // 1) If the kura is on the ground
    // 2) If the kura is bumped into a wall and can't (shouldn't) move forward
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

    public enum KuraGameMode
    {
        ClasicMode,
        SpactatorMode
    }

    ///SpactatorMode
    public int currentSpactatorModeIndex = -1;

    // ?

    public const float playerStartHealth = 100;

    [SerializeField]
    public GameObject MainCamera;

    public NetworkVariable<bool> finishedGame = new NetworkVariable<bool>(false);
    public NetworkVariable<int> playerID = new NetworkVariable<int>(-1);
    public NetworkVariable<int> placeInGame = new NetworkVariable<int>(-1);
    public NetworkVariable<float> playerRunTime = new NetworkVariable<float>(0);
    public NetworkVariable<KuraState> state = new NetworkVariable<KuraState>(KuraState.Fall);
    public NetworkVariable<KuraGameMode> gameMode = new NetworkVariable<KuraGameMode>(KuraGameMode.ClasicMode);
    public NetworkVariable<Vector3> spawnPosition = new NetworkVariable<Vector3>();
    public NetworkVariable<float> playerHealth = new NetworkVariable<float>(playerStartHealth);

    public void Update()
    {
        if (IsServer)
        {
            playerRunTime.Value += Time.deltaTime;
        }
    }
}
