using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine.UI;
using System;

// Class to manage the UI of the player
public class PlayerUIManager : NetworkBehaviour
{
    private PlayerData m_PlayerData;

    private GameData m_GameData;

    private GameObject m_StartGameButton;

    private TextMeshProUGUI m_PlayerIDText;

    private TextMeshProUGUI m_WinnerText;

    private TextMeshProUGUI m_RunTimeText;

    private TextMeshProUGUI m_LobbyIDText;

    private TextMeshProUGUI m_KuraStatetext;

    private GameObject m_SpactatorModeButton;

    private GameObject m_SpactatorModeHolder;

    [SerializeField]
    public GameObject CameraHolder;

    [SerializeField]
    private Camera m_MainCamera;

    [SerializeField]
    private Camera m_MiniMapCamera;

    [SerializeField]
    private GameObject m_MiniMapGameObject;

    private void Start()
    {
        m_GameData = GameObject.FindObjectOfType<GameData>();

        m_PlayerData = GetComponent<PlayerData>();

        m_StartGameButton = m_GameData.sceneObjectsCache.startButtonGameObject;
        m_PlayerIDText = m_GameData.sceneObjectsCache.playerIDText;
        m_WinnerText = m_GameData.sceneObjectsCache.winnerText;
        m_RunTimeText = m_GameData.sceneObjectsCache.playerRunTimeText;
        m_LobbyIDText = m_GameData.sceneObjectsCache.lobbyIDText;
        m_KuraStatetext = m_GameData.sceneObjectsCache.kuraStatetext;
        m_MiniMapGameObject = m_GameData.sceneObjectsCache.miniMapGameObject;
        m_SpactatorModeButton = m_GameData.sceneObjectsCache.SpactatorModeButton;
        m_SpactatorModeHolder = m_GameData.sceneObjectsCache.SpactatorModeHolder;

        if (IsHost)
        {
            m_StartGameButton.SetActive(true);
        }

        if (IsClient && IsOwner)
        {
            m_PlayerIDText.gameObject.SetActive(true);
            m_PlayerIDText.text = m_PlayerData.playerID.Value.ToString();

            m_RunTimeText.gameObject.SetActive(true);

            m_LobbyIDText.gameObject.SetActive(true);
            m_LobbyIDText.text = m_GameData.lobbyCode.Value.Value;

            m_KuraStatetext.gameObject.SetActive(true);

            if (!m_MainCamera.gameObject.activeInHierarchy)
            {
                m_MainCamera.gameObject.SetActive(true);
            }

            if (!m_MiniMapCamera.gameObject.activeInHierarchy)
            {
                m_MiniMapCamera.gameObject.SetActive(true);
            }

            m_MiniMapGameObject.SetActive(true);
            m_SpactatorModeButton.GetComponent<Button>().onClick.AddListener(ActivateSpactatorMode);
        }

        m_GameData.isGameRunning.OnValueChanged += OnIsGameRunningChanged;
        m_PlayerData.placeInGame.OnValueChanged += OnPlaceInGameChanged;
        m_PlayerData.state.OnValueChanged += OnKuraStateChanged;
    }

    private void OnIsGameRunningChanged(bool previous, bool current)
    {
        if (current == true)
        {
            m_StartGameButton.SetActive(false);
        }
    }

    private void OnPlaceInGameChanged(int previous, int current)
    {
        if (IsClient && IsOwner)
        {
            if (m_PlayerData.placeInGame.Value != -1)
            {
                m_WinnerText.gameObject.SetActive(true);
                m_WinnerText.text = "YOU WON " + m_PlayerData.placeInGame.Value.ToString() + " PLACE!!!";
                m_SpactatorModeButton.SetActive(true);
                m_PlayerIDText.gameObject.SetActive(false);
                m_LobbyIDText.gameObject.SetActive(false);
                m_KuraStatetext.gameObject.SetActive(false);
            }
        }
    }

    private void OnKuraStateChanged(PlayerData.KuraState previous, PlayerData.KuraState current)
    {
        if (IsClient && IsOwner)
        {
            m_KuraStatetext.text = m_PlayerData.state.Value.ToString();
        }
    }

    public void ActivateSpactatorMode()
    {
        m_SpactatorModeButton.SetActive(false);
        m_SpactatorModeHolder.gameObject.SetActive(true);
        UpdateGameModeServerRpc(PlayerData.KuraGameMode.SpactatorMode);
        m_PlayerData.currentSpactatorModeIndex = m_GameData.FindSpactatorModeIndex(m_PlayerData.currentSpactatorModeIndex);

        m_MainCamera.gameObject.SetActive(false);
        m_GameData.playerDataList[m_PlayerData.currentSpactatorModeIndex].MainCamera.SetActive(true);
    }

    [ServerRpc]
    public void UpdateGameModeServerRpc(PlayerData.KuraGameMode KuraGameMode)
    {
        m_PlayerData.gameMode.Value = KuraGameMode;
    }

    private void Update()
    {
        String temp = Math.Floor(m_PlayerData.playerRunTime.Value / 60f).ToString() + ":" + Math.Floor(m_PlayerData.playerRunTime.Value % 60f).ToString() + "." + Math.Floor(m_PlayerData.playerRunTime.Value * 10) % 10 + Math.Floor(m_PlayerData.playerRunTime.Value * 100) % 10;
        m_GameData.sceneObjectsCache.playerRunTimeText.text = temp;
    }
}
