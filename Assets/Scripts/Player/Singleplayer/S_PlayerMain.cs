using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.SceneManagement;

public class S_PlayerMain : MonoBehaviour, IPlayerMain
{
    #region Player scripts
    private S_PlayerGeneral m_General;
    private S_PlayerMovement m_Movement;
    private S_PlayerInteraction m_Interaction;
    private S_PlayerUI m_UI;
    #endregion

    [SerializeField] Transform m_Skin;

    private void Awake()
    {
        m_General = new();
        m_Movement = new(gameObject, m_Skin);
        m_Interaction = new();
        m_UI = new();

        m_General.ConnectComponents(m_Movement);
        m_Movement.ConnectComponents(m_General, m_Interaction);
        m_UI.ConnectComponents(m_General, m_Movement);

        Vector3 spawnPos = S_MainManager.Instance.GetSpawnPosition();
        m_General.SetInitialData(spawnPos);
    }

    private void Update()
    {
        if (m_General.TakeInput())
            m_Movement.ProcessInput();

        m_General.UpdateTimers();
        m_UI.UpdateUI();

        KuraTransfromData data = m_Movement.GetTransformData();
        Debug.DrawLine(data.position, new Vector3(data.position.x + data.velocity.x, data.position.y, data.position.z), Color.red, 1 / 300f);
    }

    private void FixedUpdate()
    {
        m_General.State = m_Movement.GetNewKuraState();
        m_Movement.ProcessMovement();

        m_General.Damage(m_Interaction.GetPeriodicDamageFromPlatmorms());
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (m_Interaction.OnCollisionEnter2D(collision, m_Movement.GetTransformData().position))
            m_Movement.GiveFlip();

        foreach (ContactPoint2D contact in collision.contacts)
        {
            Debug.DrawLine(new Vector3(contact.point.x, contact.point.y, transform.position.z), transform.position, Color.green, 2, false);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        m_Interaction.OnCollisionExit2D(collision);
    }

    #region Public interfaces
    public void Finish()
    {
        m_General.Finish();
        m_Movement.Finish();
        m_UI.Finish();
    }
    public void Damage(float damage)
    {
        m_General.Damage(damage);
    }
    public bool SetCheckPoint(KuraTransfromData spawnData)
    {
        m_General.SetCheckPoint(spawnData);

        return true;
    }
    public bool SetCheckPoint()
    {
        m_General.SetCheckPoint();

        return true;
    }
    public void Boost(SpeedBoostScriptableObject speedBoostData) { m_Movement.Boost(speedBoostData); }
    #endregion
}
