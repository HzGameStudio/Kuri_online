using UnityEngine;
using Unity.Netcode;
using System;

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
    public float lastCPRunTime;
    public float perfectRunTime;
    public KuraState state;
    public KuraGameMode gameMode;
    public float health;
    public float startHealth;
    public int spectatorIndex;
    public KuraTransfromData spawnData;

    public KuraData(bool finishedGameIn,
                    int playerIDIn,
                    int placeInGameIn,
                    float playerRunTimeIn,
                    float lastCPRunTimeIn,
                    float perfectRunTimeIn,
                    KuraState stateIn,
                    KuraGameMode gameModeIn,
                    float healthIn,
                    float startHealthIn,
                    int spectatorIndexIn,
                    KuraTransfromData spawnDataIn)
    {
        finishedGame = finishedGameIn;
        playerID = playerIDIn;
        placeInGame = placeInGameIn;
        playerRunTime = playerRunTimeIn;
        lastCPRunTime = lastCPRunTimeIn;
        perfectRunTime = perfectRunTimeIn;
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
        serializer.SerializeValue(ref perfectRunTime);
        serializer.SerializeValue(ref state);
        serializer.SerializeValue(ref gameMode);
        serializer.SerializeValue(ref health);
        serializer.SerializeValue(ref startHealth);
        serializer.SerializeValue(ref spectatorIndex);
        serializer.SerializeValue(ref spawnData);
    }
}

public abstract class PlayerGeneralBase
{
    // NOTE: maybe swap all public methods for protected

    private PlayerMovementBase m_MovementBase;

    protected KuraData localData;

    public KuraData LocalData { get { return localData; } }
    public KuraState State { set { localData.state = value; } }

    public Action OnRespawn;

    protected PlayerGeneralBase()
    {
        localData = new KuraData(false, -1, -1, 0, 0, 0, KuraState.None, KuraGameMode.ClasicMode, 100, 100, -1, new (Vector3.zero, Vector3.zero, 1, 2, 1) );
    }

    public void ConnectComponents(PlayerMovementBase movementBase)
    {
        m_MovementBase = movementBase;
    }

    public void UpdateTimers()
    {
        if (!localData.finishedGame)
        {
            localData.playerRunTime += Time.deltaTime;
            localData.perfectRunTime += Time.deltaTime;
        }
    }

    public bool TakeInput()
    {
        if (!(localData.gameMode == KuraGameMode.ClasicMode))
            return false;

        if (localData.finishedGame)
            return false;

        if ((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) || Input.GetMouseButtonDown(0))
        {
            return true;
        }

        return false;
    }

    public void Finish()
    {
        MainManager.Instance.numFinishedPlayers += 1;
        localData.placeInGame = MainManager.Instance.numFinishedPlayers;
        localData.finishedGame = true;
    }

    public void Damage(float damage)
    {
        localData.health -= damage;
        CheckHealth();
    }

    private void CheckHealth()
    {
        if (localData.health <= 0)
        {
            //Dead

            // TODO: Do a audio manager 
            AudioManager.Instance.PlaySFX("Death", 10f);

            OnRespawn.Invoke();

            Respawn();
        }
    }

    private void Respawn()
    {
        localData.health = localData.startHealth;
        localData.perfectRunTime = localData.lastCPRunTime;
    }

    // used in CheckPointScript
    public void SetCheckPoint(KuraTransfromData spawnData)
    {
        localData.spawnData = spawnData;
        localData.lastCPRunTime = localData.perfectRunTime;
    }

    // used in CheckPointSaveScript
    public void SetCheckPoint()
    {
        KuraTransfromData tr = m_MovementBase.GetTransformData();

        localData.spawnData = tr;
        localData.lastCPRunTime = localData.perfectRunTime;
    }

    public void SetInitialData(Vector3 pos)
    {
        localData.spawnData.position = pos;
    }
}
