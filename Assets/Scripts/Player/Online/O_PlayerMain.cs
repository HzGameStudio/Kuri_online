using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class O_PlayerMain : NetworkBehaviour, IPlayerMain
{
    [DoNotSerialize, HideInInspector]
    private NetworkVariable<KuraData> m_ServerData = new ();
    [DoNotSerialize, HideInInspector]
    private NetworkVariable<KuraTransfromData> m_TransformFromClient = new ();

    #region Player scripts
    private O_PlayerGeneral m_General;
    private O_PlayerMovement m_Movement;
    private O_PlayerInteraction m_Interaction;
    private O_PlayerUI m_UI;
    #endregion

    [SerializeField] GameObject m_BlueKura;
    [SerializeField] GameObject m_RedKura;

    [SerializeField] Transform m_Skin;

    [SerializeField] GameObject m_CameraHolder;

    public KuraData ServerData { get { return m_General.ServerData; } }

    private void Awake()
    {
        m_General = new(m_BlueKura, m_RedKura);
        m_Movement = new(gameObject, m_Skin);
        m_Interaction = new();
        m_UI = new(m_CameraHolder);

        m_General.ConnectComponents(m_Movement);
        m_Movement.ConnectComponents(m_General, m_Interaction);
        m_Interaction.ConnectComponents(m_Movement, m_General);
        m_UI.ConnectComponents(m_General, m_Movement);

        //MainManager.Instance.PlayerMainList.Add(this);

        MainManager.Instance.sceneObjectsCache.SpectatorModeButton.GetComponent<Button>().onClick.AddListener(ActivateSpactatorMode);
        MainManager.Instance.sceneObjectsCache.SpectatorModeHolder.GetComponentInChildren<Button>().onClick.AddListener(SpectateNextPlayer);
        MainManager.Instance.sceneObjectsCache.restartButton.GetComponent<Button>().onClick.AddListener(RestartGame);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        m_General.OnNetworkSpawn(m_ServerData, IsOwner);
        m_Movement.OnNetworkSpawn(m_TransformFromClient);

        m_TransformFromClient.OnValueChanged += OnTFCChanged;
    }

    private void Update()
    {
        if (m_General.TakeInput())
        {
             m_Movement.ProcessInput();
        }

        m_General.UpdateTimers(IsOwner);
        m_UI.UpdateUI(IsOwner);

        KuraTransfromData data = m_Movement.GetTransformData();
        Debug.DrawLine(data.position, new Vector3(data.position.x + data.velocity.x, data.position.y, data.position.z), Color.red, 1 / 300f);
    }

    private void FixedUpdate()
    {
        m_General.State = m_Movement.GetNewKuraState();
        m_Movement.ProcessMovement();

        m_Interaction.TakePeriodicDamageFromPlatmorms(IsOwner);

        if (IsOwner)
            SyncDataServerRPC(m_General.LocalData, m_Movement.GetTransformData());
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        m_Interaction.OnCollisionEnter2D(collision);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        m_Interaction.OnCollisionExit2D(collision);
    }

    public void ActivateSpactatorMode()
    {
        m_General.ActivateSpactatorMode();
        m_UI.ActivateSpectatorMode();

        m_UI.ChangeSpectateCamera(m_General.SpectateNextPlayer());
    }

    public void SpectateNextPlayer()
    {
        m_UI.ChangeSpectateCamera(m_General.SpectateNextPlayer());
    }

    public void ActivateCamera()
    {
        m_UI.ActivateCamera();
    }

    public void DeactivateCamera()
    {
        m_UI.DeactivateCamera();
    }

    [ServerRpc]
    private void SyncDataServerRPC(KuraData localData, KuraTransfromData transformData)
    {
        m_General.UpdateDataOnServerRPC(localData);
        m_Movement.UpdatePositionOnServerRPC(transformData, IsOwner);
    }

    [ClientRpc]
    public void SetInitialDataToClientRPC(int ID, Vector3 pos, ClientRpcParams clientRpcParams = default)
    {
        m_General.SetInitialData(ID, pos);

        m_UI.SetupUI(IsOwner);
    }

    #region Public interfaces
    public void Finish()
    {
        m_General.Finish(IsOwner);
        m_Movement.Finish();
        m_UI.Finish();
    }
    public void Damage(float damage) { m_General.Damage(damage, IsOwner); }
    public bool SetCheckPoint(KuraTransfromData spawnData) { return m_General.SetCheckPoint(spawnData, IsOwner); }
    public bool SetCheckPoint() { return m_General.SetCheckPoint(IsOwner); }
    public void Boost(SpeedBoostScriptableObject speedBoostData) { m_Movement.Boost(speedBoostData); }
    public void RestartGame() { LoadingSceneManager.Instance.LoadScene(SceneName.O_GameMenu, true); }
    #endregion

    void OnTFCChanged(KuraTransfromData previous, KuraTransfromData current)
    {
        // updates data on all clients except server, all kuras except own kura

        if (IsOwner || IsServer)
            return;

        m_Movement.ApplyTransformData(current);
    }
}
