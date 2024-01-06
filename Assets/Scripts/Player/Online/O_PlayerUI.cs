using TMPro;
using UnityEngine;
using System;
using UnityEditor;

public class O_PlayerUI : PlayerUIBase
{
    private O_PlayerGeneral m_GeneralBase;
    private O_PlayerMovement m_Movement;

    #region Canvas element references
    private readonly TextMeshProUGUI m_PlayerIDText;
    private readonly TextMeshProUGUI m_WinnerText;
    private readonly TextMeshProUGUI m_RunTimeText;
    private readonly TextMeshProUGUI m_PerfectRunTimeText;
    private readonly TextMeshProUGUI m_LobbyIDText;
    private readonly TextMeshProUGUI m_KuraSpeedtext;
    private readonly GameObject m_SpactatorModeButton;
    private readonly GameObject m_SpactatorModeHolder;
    private readonly GameObject m_MiniMapGameObject;
    private readonly GameObject m_RestartButton;
    #endregion

    private readonly GameObject m_CameraHolder;

    public O_PlayerUI(GameObject cameraHolder)
    {
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

        m_CameraHolder = cameraHolder;
    }

    public void ConnectComponents(O_PlayerGeneral general, O_PlayerMovement movement)
    {
        m_GeneralBase = general;
        m_Movement = movement;
    }

    public void SetupUI()
    {
        m_PlayerIDText.text = m_GeneralBase.LocalData.playerID.ToString();

        m_LobbyIDText.text = GameManager.Instance.lobbyCode.Value.Value;

        m_CameraHolder.SetActive(true);
    }

    public void UpdateUI()
    {
        m_KuraSpeedtext.text = Math.Floor(m_Movement.Velocity.x).ToString();

        m_RunTimeText.text = Math.Floor(m_GeneralBase.LocalData.playerRunTime / 60f).ToString() + ":" + Math.Floor(m_GeneralBase.LocalData.playerRunTime % 60f).ToString() + "." + Math.Floor(m_GeneralBase.LocalData.playerRunTime * 10) % 10 + Math.Floor(m_GeneralBase.LocalData.playerRunTime * 100) % 10;
        m_PerfectRunTimeText.text = Math.Floor(m_GeneralBase.LocalData.perfectRunTime / 60f).ToString() + ":" + Math.Floor(m_GeneralBase.LocalData.perfectRunTime % 60f).ToString() + "." + Math.Floor(m_GeneralBase.LocalData.perfectRunTime * 10) % 10 + Math.Floor(m_GeneralBase.LocalData.perfectRunTime * 100) % 10;
    }

    public void Finish(bool IsOwner, bool IsServer)
    {
        if (IsServer)
        {
            if (MainManager.Instance.numFinishedPlayers == MainManager.Instance.numPlayersInGame.Value)
                m_RestartButton.SetActive(true);
        }

        if (!IsOwner) return;

        m_WinnerText.gameObject.SetActive(true);
        m_WinnerText.text = "YOU WON " + m_GeneralBase.LocalData.placeInGame.ToString() + " PLACE!!!";
        m_SpactatorModeButton.SetActive(true);

        // m_PlayerIDText.gameObject.SetActive(false);
        // m_LobbyIDText.gameObject.SetActive(false);
        m_KuraSpeedtext.gameObject.SetActive(false);
    }

    public void ActivateSpectatorMode()
    {
        m_SpactatorModeButton.SetActive(false);
        m_SpactatorModeHolder.gameObject.SetActive(true);

        m_WinnerText.gameObject.SetActive(false);
        m_RunTimeText.gameObject.SetActive(false);
    }

    public void ChangeSpectateCamera(int prev)
    {
        if (m_GeneralBase.LocalData.spectatorIndex == -1)
            return;

        // deactive own camera (for when spectator mode is activating)
        m_CameraHolder.gameObject.SetActive(false);

        if (prev != -1)
        {
            MainManager.Instance.PlayerMainList[prev].DeactivateCamera();
        }

        MainManager.Instance.PlayerMainList[m_GeneralBase.LocalData.spectatorIndex].ActivateCamera();
    }

    public void ActivateCamera()
    {
        m_CameraHolder.gameObject.SetActive(true);
    }

    public void DeactivateCamera()
    {
        m_CameraHolder.gameObject.SetActive(false);
    }
}
