using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using System.Runtime.InteropServices;
using Unity.Collections;

// This class manages the UI of the player
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

    [SerializeField]
    private Camera m_MainCamera;

    [SerializeField]
    private Camera m_MiniMapCamera;

    private GameObject m_MiniMapGameObject;

    private void Start()
    {
        m_GameData = GameObject.FindGameObjectWithTag("gameManager").GetComponent<GameData>();

        m_StartGameButton = m_GameData.startButtonGameObject;
        m_PlayerIDText = m_GameData.playerIDText;
        m_WinnerText = m_GameData.winnerText;
        m_RunTimeText = m_GameData.playerRunTimeText;
        m_LobbyIDText = m_GameData.lobbyIDText;
        m_KuraStatetext = m_GameData.kuraStatetext;

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
        }

        // <NetworkVariable>s have the ability to call functions when their value is changed (this is pretty cool yo)
        // This is how you bind a function to call when the value changes,
        // the function will always be given the previous value of the <NetworkVariable> and the current
        m_GameData.isGameRunning.OnValueChanged += OnIsGameRunningChanged;
        m_PlayerData.placeInGame.OnValueChanged += OnPlaceInGameChanged;
        m_PlayerData.state.OnValueChanged += OnKuraStateChanged;
    }

    // The following functions get called when a value that's displayed on the UI is changed and change the UI,
    // Pretty self-explanatory

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
}
