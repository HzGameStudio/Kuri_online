using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using Unity.VisualScripting;
using System;
using UnityEngine.UI;


public struct SpawnPointData
{
    int gravityDerection;
    Vector3 velosity;
    Vector3 position;

}
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

[Serializable]
public struct KuraData : INetworkSerializable
{
    public bool finishedGame;
    public int playerID;
    public int placeInGame;
    public float playerRunTime;
    public KuraState state;
    public KuraGameMode gameMode;
    public float health;
    public float startHealth;
    public int spectatorIndex;
    public PlayerMovementManager.KuraTransfromData spawnData;

    public KuraData(bool finishedGameIn,
                    int playerIDIn,
                    int placeInGameIn,
                    float playerRunTimeIn,
                    KuraState stateIn,
                    KuraGameMode gameModeIn,
                    float healthIn,
                    float startHealthIn,
                    int spectatorIndexIn,
                    PlayerMovementManager.KuraTransfromData spawnDataIn)
    {
        finishedGame = finishedGameIn;
        playerID = playerIDIn;
        placeInGame = placeInGameIn;
        playerRunTime = playerRunTimeIn;
        state = stateIn;
        gameMode = gameModeIn;
        health = healthIn;
        startHealth = startHealthIn;
        spectatorIndex = spectatorIndexIn;
        spawnData = spawnDataIn;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref finishedGame);
        serializer.SerializeValue(ref playerID);
        serializer.SerializeValue(ref placeInGame);
        serializer.SerializeValue(ref playerRunTime);
        serializer.SerializeValue(ref state);
        serializer.SerializeValue(ref gameMode);
        serializer.SerializeValue(ref health);
        serializer.SerializeValue(ref startHealth);
        serializer.SerializeValue(ref spectatorIndex);
        serializer.SerializeValue(ref spawnData);
    }
}

public class PlayerMain : NetworkBehaviour
{
    // make starting kura data
    // and checkpoint kura data

    [SerializeField]
    public KuraData localData;

    [HideInInspector]
    public NetworkVariable<KuraData> serverData = new NetworkVariable<KuraData>();

    private PlayerMovementManager m_PlayerMovementManager;
    private PlayerUIManager m_PlayerUIManager;

    private void Awake()
    {
        localData = new KuraData(false, -1, -1, 0, KuraState.Fall, KuraGameMode.ClasicMode, 100, 100, -1, new PlayerMovementManager.KuraTransfromData(Vector3.zero, 1, 1, Vector3.zero));

        m_PlayerMovementManager = gameObject.GetComponent<PlayerMovementManager>();
        m_PlayerUIManager = gameObject.GetComponent<PlayerUIManager>();

        MainManager.Instance.PlayerMainList.Add(this);

        MainManager.Instance.sceneObjectsCache.SpectatorModeButton.GetComponent<Button>().onClick.AddListener(ActivateSpactatorMode);
        MainManager.Instance.sceneObjectsCache.SpectatorModeHolder.GetComponentInChildren<Button>().onClick.AddListener(SpectateNextPlayer);
        MainManager.Instance.sceneObjectsCache.restartButton.GetComponent<Button>().onClick.AddListener(RestartGame);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        serverData.Value = localData;
    }

    private void Update()
    {
        if (IsClient && IsOwner && !localData.finishedGame)
        {
            localData.playerRunTime += Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        if (IsClient && IsOwner)
        {


            UpdateDataOnServerRPC(localData);
        }
    }

    public void Finish()
    {
        if (!(IsClient && IsOwner))
            return;

        localData.placeInGame = MainManager.Instance.numFinishedPlayers;
        localData.finishedGame = true;

        m_PlayerMovementManager.Finish();
        m_PlayerUIManager.Finish();
    }

    public void Damage(float damage)
    {
        if (!(IsClient && IsOwner))
            return;

        localData.health -= damage;
        CheckHealth();
    }

    private void CheckHealth()
    {
        if (localData.health <= 0)
        {
            //Dead
            Respawn();
        }
    }

    private void Respawn()
    {
        localData.health = localData.startHealth;
        m_PlayerMovementManager.Respawn();
        //here we should use struct and assign velosity and gravity
    }

    public void ActivateSpactatorMode()
    {
        localData.gameMode = KuraGameMode.SpactatorMode;
        
        m_PlayerUIManager.ActivateSpectatorMode();

        SpectateNextPlayer();
    }

    public void SpectateNextPlayer()
    {
        int prev = localData.spectatorIndex;

        localData.spectatorIndex = MainManager.Instance.FindSpactatorModeIndex(localData.spectatorIndex);

        m_PlayerUIManager.ChangeSpectateCamera(prev);
    }

    public void ActivateCamera()
    {
        m_PlayerUIManager.ActivateCamera();
    }

    public void DeactivateCamera()
    {
        m_PlayerUIManager.DeactivateCamera();
    }

    public void RestartGame()
    {
        LoadingSceneManager.Instance.LoadScene(SceneName.GameMenu, true);
    }

    [ServerRpc]
    private void UpdateDataOnServerRPC(KuraData localData)
    {
        serverData.Value = localData;
    }

    [ClientRpc]
    public void SendKuraDataToClientRPC(int ID, Vector3 pos, ClientRpcParams clientRpcParams = default)
    {
        localData.playerID = ID;
        localData.spawnData.position = pos;
    }

    public bool isLocalPlayer()
    {
        return IsClient && IsOwner;
    }
    public bool SetCheckPoint(Vector3 spawPos, Vector3 velocity, float gravity, int gravity_dir)
    {
        if (!(IsClient && IsOwner))
            return false;

        localData.spawnData.position = spawPos;
        localData.spawnData.velocity = velocity;
        localData.spawnData.gravityMultiplier = gravity;
        localData.spawnData.gravityDirection = gravity_dir;

        return true;

    }
}
