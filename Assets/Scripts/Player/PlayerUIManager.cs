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
    private PlayerMain m_PlayerMain;

    private TextMeshProUGUI m_PlayerIDText;

    private TextMeshProUGUI m_WinnerText;

    private TextMeshProUGUI m_RunTimeText;

    private TextMeshProUGUI m_LobbyIDText;

    private TextMeshProUGUI m_KuraStatetext;

    private GameObject m_SpactatorModeButton;

    private GameObject m_SpactatorModeHolder;

    private GameObject m_MiniMapGameObject;

    [SerializeField]
    public GameObject CameraHolder;

    [SerializeField]
    private Camera m_MainCamera;

    [SerializeField]
    private Camera m_MiniMapCamera;

    private void Start()
    {
        m_PlayerMain = GetComponent<PlayerMain>();

        m_PlayerIDText = MainManager.Instance.sceneObjectsCache.playerIDText;
        m_WinnerText = MainManager.Instance.sceneObjectsCache.winnerText;
        m_RunTimeText = MainManager.Instance.sceneObjectsCache.playerRunTimeText;
        m_LobbyIDText = MainManager.Instance.sceneObjectsCache.lobbyIDText;
        m_KuraStatetext = MainManager.Instance.sceneObjectsCache.kuraStatetext;
        m_MiniMapGameObject = MainManager.Instance.sceneObjectsCache.miniMapGameObject;
        m_SpactatorModeButton = MainManager.Instance.sceneObjectsCache.SpectatorModeButton;
        m_SpactatorModeHolder = MainManager.Instance.sceneObjectsCache.SpectatorModeHolder;

        if (IsClient && IsOwner)
        {
            m_PlayerIDText.gameObject.SetActive(true);
            m_PlayerIDText.text = m_PlayerMain.localData.playerID.ToString();

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
        }
    }

    private void Update()
    {
        if (!(IsClient && IsOwner))
            return;

        m_KuraStatetext.text = m_PlayerMain.localData.state.ToString();

        m_RunTimeText.text = Math.Floor(m_PlayerMain.localData.playerRunTime / 60f).ToString() + ":" + Math.Floor(m_PlayerMain.localData.playerRunTime % 60f).ToString() + "." + Math.Floor(m_PlayerMain.localData.playerRunTime * 10) % 10 + Math.Floor(m_PlayerMain.localData.playerRunTime * 100) % 10;    }

    public void Finish()
    {
        m_WinnerText.gameObject.SetActive(true);
        m_WinnerText.text = "YOU WON " + m_PlayerMain.localData.placeInGame.ToString() + " PLACE!!!";
        m_SpactatorModeButton.SetActive(true);

        m_PlayerIDText.gameObject.SetActive(false);
        m_LobbyIDText.gameObject.SetActive(false);
        m_KuraStatetext.gameObject.SetActive(false);
    }

    public void ActivateSpectatorMode()
    {
        m_SpactatorModeButton.SetActive(false);
        m_SpactatorModeHolder.gameObject.SetActive(true);
    }

    public void ChangeSpectateCamera(int prev)
    {
        if (m_PlayerMain.localData.spectatorIndex == -1)
            return;
        
        // deactive own camera (for when spectator mode is activating)
        m_MainCamera.gameObject.SetActive(false);

        if (prev != -1)
            MainManager.Instance.PlayerMainList[prev].DeactivateCamera();

        MainManager.Instance.PlayerMainList[m_PlayerMain.localData.spectatorIndex].ActivateCamera();
    }

    public void ActivateCamera()
    {
        m_MainCamera.gameObject.SetActive(true);
    }

    public void DeactivateCamera()
    {
        m_MainCamera.gameObject.SetActive(false);
    }
}
