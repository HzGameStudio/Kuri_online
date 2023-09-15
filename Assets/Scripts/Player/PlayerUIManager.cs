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
        m_PlayerData = GetComponent<PlayerData>();

        m_PlayerIDText = MainManager.Instance.sceneObjectsCache.playerIDText;
        m_WinnerText = MainManager.Instance.sceneObjectsCache.winnerText;
        m_RunTimeText = MainManager.Instance.sceneObjectsCache.playerRunTimeText;
        m_LobbyIDText = MainManager.Instance.sceneObjectsCache.lobbyIDText;
        m_KuraStatetext = MainManager.Instance.sceneObjectsCache.kuraStatetext;
        m_MiniMapGameObject = MainManager.Instance.sceneObjectsCache.miniMapGameObject;
        m_SpactatorModeButton = MainManager.Instance.sceneObjectsCache.SpactatorModeButton;
        m_SpactatorModeHolder = MainManager.Instance.sceneObjectsCache.SpactatorModeHolder;

        if (IsClient && IsOwner)
        {
            m_PlayerIDText.gameObject.SetActive(true);
            m_PlayerIDText.text = m_PlayerData.playerID.Value.ToString();

            m_RunTimeText.gameObject.SetActive(true);

            m_LobbyIDText.gameObject.SetActive(true);
            m_LobbyIDText.text = GameManager.Instance.lobbyCode.Value.Value;

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
        m_PlayerData.currentSpactatorModeIndex = MainManager.Instance.FindSpactatorModeIndex(m_PlayerData.currentSpactatorModeIndex);

        m_MainCamera.gameObject.SetActive(false);
        MainManager.Instance.playerDataList[m_PlayerData.currentSpactatorModeIndex].MainCamera.SetActive(true);
    }

    [ServerRpc]
    public void UpdateGameModeServerRpc(PlayerData.KuraGameMode KuraGameMode)
    {
        m_PlayerData.gameMode.Value = KuraGameMode;
    }

    private void Update()
    {
        String temp = Math.Floor(m_PlayerData.playerRunTime.Value / 60f).ToString() + ":" + Math.Floor(m_PlayerData.playerRunTime.Value % 60f).ToString() + "." + Math.Floor(m_PlayerData.playerRunTime.Value * 10) % 10 + Math.Floor(m_PlayerData.playerRunTime.Value * 100) % 10;
        MainManager.Instance.sceneObjectsCache.playerRunTimeText.text = temp;
    }
}
