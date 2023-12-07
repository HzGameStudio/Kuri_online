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

    private TextMeshProUGUI m_PerfectRunTimeText;

    private TextMeshProUGUI m_LobbyIDText;

    private TextMeshProUGUI m_KuraSpeedtext;

    private GameObject m_SpactatorModeButton;

    private GameObject m_SpactatorModeHolder;

    private GameObject m_MiniMapGameObject;

    [SerializeField]
    private GameObject m_CameraHolder;

    private GameObject m_RestartButton;

    private void Start()
    {
        m_PlayerMain = GetComponent<PlayerMain>();

        m_PlayerIDText = MainManager.Instance.sceneObjectsCache.playerIDText;
        m_WinnerText = MainManager.Instance.sceneObjectsCache.winnerText;
        m_RunTimeText = MainManager.Instance.sceneObjectsCache.playerRunTimeText;
        m_PerfectRunTimeText = MainManager.Instance.sceneObjectsCache.perfectRunTimeText;
        m_LobbyIDText = MainManager.Instance.sceneObjectsCache.lobbyIDText;
        m_KuraSpeedtext = MainManager.Instance.sceneObjectsCache.kuraSpeedtext;
        m_MiniMapGameObject = MainManager.Instance.sceneObjectsCache.miniMapGameObject;
        m_SpactatorModeButton = MainManager.Instance.sceneObjectsCache.SpectatorModeButton;
        m_SpactatorModeHolder = MainManager.Instance.sceneObjectsCache.SpectatorModeHolder;
        m_RestartButton = MainManager.Instance.sceneObjectsCache.restartButton;

        if (IsClient && IsOwner)
        {
            m_PlayerIDText.gameObject.SetActive(true);
            m_PlayerIDText.text = m_PlayerMain.localData.playerID.ToString();

            m_RunTimeText.gameObject.SetActive(true);
            m_PerfectRunTimeText.gameObject.SetActive(true);

            m_LobbyIDText.gameObject.SetActive(true);
            m_LobbyIDText.text = GameManager.Instance.lobbyCode.Value.Value;

            m_KuraSpeedtext.gameObject.SetActive(true);

            if (!m_CameraHolder.gameObject.activeInHierarchy)
            {
                m_CameraHolder.gameObject.SetActive(true);
            }

            m_MiniMapGameObject.SetActive(true);
        }
    }

    private void Update()
    {
        if (!(IsClient && IsOwner))
            return;

        m_KuraSpeedtext.text = Math.Floor(m_PlayerMain.GetVelocity().x).ToString();

        m_RunTimeText.text = Math.Floor(m_PlayerMain.localData.playerRunTime / 60f).ToString() + ":" + Math.Floor(m_PlayerMain.localData.playerRunTime % 60f).ToString() + "." + Math.Floor(m_PlayerMain.localData.playerRunTime * 10) % 10 + Math.Floor(m_PlayerMain.localData.playerRunTime * 100) % 10;
        m_PerfectRunTimeText.text = Math.Floor(m_PlayerMain.localData.perfectRunTime / 60f).ToString() + ":" + Math.Floor(m_PlayerMain.localData.perfectRunTime % 60f).ToString() + "." + Math.Floor(m_PlayerMain.localData.perfectRunTime * 10) % 10 + Math.Floor(m_PlayerMain.localData.perfectRunTime * 100) % 10;
    }

    public void Finish()
    {
        m_WinnerText.gameObject.SetActive(true);
        m_WinnerText.text = "YOU WON " + m_PlayerMain.localData.placeInGame.ToString() + " PLACE!!!";
        m_SpactatorModeButton.SetActive(true);

        // m_PlayerIDText.gameObject.SetActive(false);
        // m_LobbyIDText.gameObject.SetActive(false);
        m_KuraSpeedtext.gameObject.SetActive(false);
    }

    public void ActivateSpectatorMode()
    {
        m_SpactatorModeButton.SetActive(false);
        m_SpactatorModeHolder.gameObject.SetActive(true);

        finishedGameUIDeactivate();

    }

    public void ChangeSpectateCamera(int prev)
    {
        if (m_PlayerMain.localData.spectatorIndex == -1)
            return;
        
        // deactive own camera (for when spectator mode is activating)
        m_CameraHolder.gameObject.SetActive(false);

        if (prev != -1)
            MainManager.Instance.PlayerMainList[prev].DeactivateCamera();

        MainManager.Instance.PlayerMainList[m_PlayerMain.localData.spectatorIndex].ActivateCamera();
    }

    public void ActivateCamera()
    {
        m_CameraHolder.gameObject.SetActive(true);
    }

    public void DeactivateCamera()
    {
        m_CameraHolder.gameObject.SetActive(false);
    }

    private void finishedGameUIDeactivate()
    {
        m_WinnerText.gameObject.SetActive(false);
        m_RunTimeText.gameObject.SetActive(false);


    }
}
