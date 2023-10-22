using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using Unity.VisualScripting;
using System;
using UnityEngine.UI;

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
    public Vector3 spawnPosition;
    public float health;
    public float startHealth;
    public int spectatorIndex;

    public KuraData(bool finishedGameIn,
                    int playerIDIn,
                    int placeInGameIn,
                    float playerRunTimeIn,
                    KuraState stateIn,
                    KuraGameMode gameModeIn,
                    Vector3 spawnPositionIn,
                    float healthIn,
                    float startHealthIn,
                    int spectatorIndexIn)
    {
        finishedGame = finishedGameIn;
        playerID = playerIDIn;
        placeInGame = placeInGameIn;
        playerRunTime = playerRunTimeIn;
        state = stateIn;
        gameMode = gameModeIn;
        spawnPosition = spawnPositionIn;
        health = healthIn;
        startHealth = startHealthIn;
        spectatorIndex = spectatorIndexIn;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref finishedGame);
        serializer.SerializeValue(ref playerID);
        serializer.SerializeValue(ref placeInGame);
        serializer.SerializeValue(ref playerRunTime);
        serializer.SerializeValue(ref state);
        serializer.SerializeValue(ref gameMode);
        serializer.SerializeValue(ref spawnPosition);
        serializer.SerializeValue(ref health);
        serializer.SerializeValue(ref startHealth);
        serializer.SerializeValue(ref spectatorIndex);
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
        localData = new KuraData(false, -1, -1, 0, KuraState.Fall, KuraGameMode.ClasicMode, Vector3.zero, 100, 100, -1);

        m_PlayerMovementManager = gameObject.GetComponent<PlayerMovementManager>();
        m_PlayerUIManager = gameObject.GetComponent<PlayerUIManager>();

        MainManager.Instance.PlayerMainList.Add(this);

        MainManager.Instance.sceneObjectsCache.SpectatorModeButton.GetComponent<Button>().onClick.AddListener(ActivateSpactatorMode);
        MainManager.Instance.sceneObjectsCache.SpectatorModeHolder.GetComponentInChildren<Button>().onClick.AddListener(SpectateNextPlayer);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        serverData.Value = localData;
    }

    private void Update()
    {
        if (IsClient && IsOwner)
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

    [ServerRpc]
    private void UpdateDataOnServerRPC(KuraData localData)
    {
        serverData.Value = localData;
    }

    [ClientRpc]
    public void SendKuraDataToClientRPC(int ID, Vector3 pos, ClientRpcParams clientRpcParams = default)
    {
        Debug.Log("RPC");
        localData.playerID = ID;
        localData.spawnPosition = pos;
    }
}
