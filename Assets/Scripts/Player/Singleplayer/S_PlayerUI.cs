using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System;

public class S_PlayerUI
{
    private S_PlayerGeneral m_GeneralBase;
    private S_PlayerMovement m_Movement;

    #region Canvas element references
    private readonly TextMeshProUGUI m_WinnerText;
    private readonly TextMeshProUGUI m_RunTimeText;
    private readonly TextMeshProUGUI m_PerfectRunTimeText;
    private readonly TextMeshProUGUI m_KuraSpeedtext;
    private readonly GameObject m_RestartButton;
    #endregion

    public S_PlayerUI()
    {
        m_WinnerText = S_MainManager.Instance.sceneObjectsCache.winnerText;
        m_RunTimeText = S_MainManager.Instance.sceneObjectsCache.playerRunTimeText;
        m_PerfectRunTimeText = S_MainManager.Instance.sceneObjectsCache.perfectRunTimeText;
        m_KuraSpeedtext = S_MainManager.Instance.sceneObjectsCache.kuraSpeedtext;
        m_RestartButton = S_MainManager.Instance.sceneObjectsCache.restartButton;
    }

    public void ConnectComponents(S_PlayerGeneral general, S_PlayerMovement movement)
    {
        m_GeneralBase = general;
        m_Movement = movement;
    }

    public void UpdateUI()
    {
        m_KuraSpeedtext.text = Math.Floor(m_Movement.Velocity.x).ToString();

        m_RunTimeText.text = Math.Floor(m_GeneralBase.LocalData.playerRunTime / 60f).ToString() + ":" + Math.Floor(m_GeneralBase.LocalData.playerRunTime % 60f).ToString() + "." + Math.Floor(m_GeneralBase.LocalData.playerRunTime * 10) % 10 + Math.Floor(m_GeneralBase.LocalData.playerRunTime * 100) % 10;
        m_PerfectRunTimeText.text = Math.Floor(m_GeneralBase.LocalData.perfectRunTime / 60f).ToString() + ":" + Math.Floor(m_GeneralBase.LocalData.perfectRunTime % 60f).ToString() + "." + Math.Floor(m_GeneralBase.LocalData.perfectRunTime * 10) % 10 + Math.Floor(m_GeneralBase.LocalData.perfectRunTime * 100) % 10;
    }

    public void Finish()
    {
        m_RestartButton.SetActive(true);

        m_WinnerText.gameObject.SetActive(true);
        m_WinnerText.text = "YOU WON";

        m_KuraSpeedtext.gameObject.SetActive(false);
    }
}
