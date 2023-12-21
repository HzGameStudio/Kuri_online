using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class O_PlayerGeneral : PlayerGeneralBase
{
    [DoNotSerialize, HideInInspector]
    private NetworkVariable<KuraData> m_ServerData;
    public KuraData ServerData { get { return m_ServerData.Value; } }

    private readonly GameObject m_RedKura;
    private readonly GameObject m_BlueKura;

    public O_PlayerGeneral(GameObject blueKura, GameObject redKura) : base()
    {
        m_BlueKura = blueKura;
        m_RedKura = redKura;
    }

    public void OnNetworkSpawn(NetworkVariable<KuraData> serverData, bool IsOwner)
    {
        m_ServerData = serverData;
        m_ServerData.Value = localData;

        //this makes you see yourself as a blue kura
        //while other players are red 
        if (!IsOwner)
        {
            m_RedKura.SetActive(true);
            m_BlueKura.SetActive(false);
        }
    }

    public void UpdateTimers(bool IsOwner)
    {
        if (IsOwner)
            base.UpdateTimers();
    }

    public void Finish(bool IsOwner)
    {
        if (IsOwner)
            base.Finish();
    }

    public void Damage(float damage, bool IsOwner)
    {
        if (IsOwner)
            base.Damage(damage);
    }

    public void ActivateSpactatorMode()
    {
        localData.gameMode = KuraGameMode.SpactatorMode;
    }

    public int SpectateNextPlayer()
    {
        int prev = localData.spectatorIndex;

        localData.spectatorIndex = MainManager.Instance.FindSpactatorModeIndex(localData.spectatorIndex);

        return prev;
    }

    public bool SetCheckPoint(KuraTransfromData spawnData, bool IsOwner)
    {
        if (IsOwner)
        {
            base.SetCheckPoint(spawnData);
            return true;
        }
        return false;
    }

    public bool SetCheckPoint(bool IsOwner)
    {
        if (IsOwner)
        {
            base.SetCheckPoint();
            return true;
        }
        return false;
    }

    public void UpdateDataOnServerRPC(KuraData localData)
    {
        m_ServerData.Value = localData;
    }

    public void SetInitialData(int ID, Vector3 pos)
    {
        localData.playerID = ID;
        localData.spawnData.position = pos;
    }
}
