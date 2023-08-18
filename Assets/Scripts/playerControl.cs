using System;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;
using TMPro;
using System.Linq;
using Unity.VisualScripting;

public class PlayerControl : NetworkBehaviour
{
    // *** Constants

    // Physics

    [SerializeField]
    private float m_MaxRunVelocity;

    [SerializeField]
    private float m_MinFlyVelocity;

    [SerializeField]
    private float m_MaxFlyVelocity;

    [SerializeField]
    private float m_AbsoluteMaxVelocity;

    [SerializeField]
    private float m_ChillThresholdVelocity;



    [SerializeField]
    private float m_RunForce;

    [SerializeField]
    private float m_ReadyRunForce;

    [SerializeField]
    private float m_RunBrakeForce;

    [SerializeField]
    private float m_FlyForce;

    [SerializeField]
    private float m_FlyBrakeForce;

    [SerializeField]
    private float m_GravityMultiplier;

    // Objects

    [SerializeField]
    private Camera m_MainCamera;

    [SerializeField]
    private Camera m_MiniMapCamera;

    [SerializeField]
    private BoxCollider2D m_BoxCollider2D;

    [SerializeField]
    private Rigidbody2D m_RigidBody2d;

    // square transform, not player
    [SerializeField]
    private Transform m_Transform;

    private GameData m_GameManagerGameData;

    [SerializeField]
    private GameObject m_RedKura;

    [SerializeField]
    private GameObject m_BlueKura;

    private PlayerData m_PlayerData;

    // Logic

    [SerializeField]
    private string[] m_FlipTagList = { "simplePlatform", "player" };

    [SerializeField]
    private int m_MaxFlips = 1;

    private float m_MaxFlapRunTime = 1.5f;

    // Other

    [SerializeField]
    private Vector2 m_RangeTeleportation = new Vector2(2, 10);



    // *** Active

    private bool m_Request = false;

    private int m_GravityDirection = 1;

    private int m_NFlips = 1;

    // Platform tag, platform direction, platform gameObject name
    public List<Tuple<string, int, string>> m_TouchingPlatforms = new List<Tuple<string, int, string>>();

    private float m_CurFlapRunTime = 0f;

    private void Start()
    {
        //Time.timeScale = 0.3f;

        if (IsClient && IsOwner)
        {
            if (!m_MainCamera.gameObject.activeInHierarchy)
            {
                m_MainCamera.gameObject.SetActive(true);
            }

            if (!m_MiniMapCamera.gameObject.activeInHierarchy)
            {
                m_MiniMapCamera.gameObject.SetActive(true);
            }
        }

        if (IsClient && IsOwner)
        {
            m_RedKura.SetActive(false);
            m_BlueKura.SetActive(true);
        }
        else
        {
            m_RedKura.SetActive(true);
            m_BlueKura.SetActive(false);
        }

        m_PlayerData = GetComponent<PlayerData>();

        m_GameManagerGameData = GameObject.FindGameObjectWithTag("gameManager").GetComponent<GameData>();

        m_GameManagerGameData.playerDataList.Add(new GameData.PlayerData(gameObject, m_PlayerData.playerRunTime.Value, 0));
        m_PlayerData.finishedgame.OnValueChanged += OnFinishedGameChanged;

        if(IsOwner)
        {
            m_GameManagerGameData.MiniMapGameObject.SetActive(true);
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (IsServer)
        {
            UpdateServer();
        }

        if (IsClient && IsOwner)
        {
            UpdateClient();
        }
    }

    private void UpdateServer()
    {
        if (!(m_GameManagerGameData.isGameRunning.Value && !m_PlayerData.finishedgame.Value)) return;

        //Debug.Log(nm.ConnectedClientsList.Count);
        m_PlayerData.playerRunTime.Value += Time.deltaTime;
        if (m_Request)
        {
            m_Request = false;

            if (m_NFlips > 0)
            {
                m_GravityDirection *= -1;
                m_RigidBody2d.gravityScale = m_GravityDirection * m_GravityMultiplier;
                m_Transform.localScale = new Vector3(m_Transform.localScale.x, m_Transform.localScale.y * -1, m_Transform.localScale.z);

                m_NFlips--;
            }
        }
    }

    private void UpdateClient()
    {
        if(GetComponent<PlayerData>().finishedgame.Value == false)
        {
            if ((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) || Input.GetMouseButtonDown(0))
            {
                UpdateClientPositionServerRpc(true);
            }
        }

        // help why
        String temp = Math.Floor(m_PlayerData.playerRunTime.Value / 60f).ToString() + ":" + Math.Floor(m_PlayerData.playerRunTime.Value).ToString() + "." + Math.Floor(m_PlayerData.playerRunTime.Value * 10) % 10 + Math.Floor(m_PlayerData.playerRunTime.Value * 100) % 10;
        m_GameManagerGameData.playerRunTimeText.text = temp;

        Debug.DrawLine(transform.position, new Vector3(transform.position.x + m_RigidBody2d.velocity.x, transform.position.y, transform.position.z), Color.red, 1 / 300f);
    }

    [ServerRpc]
    public void UpdateClientPositionServerRpc(bool request)
    {
        m_Request = request;
    }

    private void FixedUpdate()
    {
        if (IsServer)
        {
            FixedUpdateServer();
        }
    }

    private void FixedUpdateServer()
    {
        if (!(m_GameManagerGameData.isGameRunning.Value && !GetComponent<PlayerData>().finishedgame.Value))
        {
            return;
        }

        bool onFloor = m_TouchingPlatforms.Any(platform => (m_GravityDirection == 1 && platform.Item2 == 2) ||
                                                           (m_GravityDirection == -1 && platform.Item2 == 0));

        bool kissWall = m_TouchingPlatforms.Any(platform => platform.Item2 == 1);

        if (kissWall)
        {
            if (onFloor)
                m_PlayerData.state.Value = PlayerData.KuraState.Stand;
            else
                m_PlayerData.state.Value = PlayerData.KuraState.Fall;
        }
        else
        {
            if (onFloor)
            {
                if (m_RigidBody2d.velocity.x < m_MinFlyVelocity)
                    m_PlayerData.state.Value = PlayerData.KuraState.Run;
                else if (m_RigidBody2d.velocity.x <= m_MaxRunVelocity)
                {
                    m_PlayerData.state.Value = PlayerData.KuraState.ReadyRun;
                }
                else
                {
                    if (m_CurFlapRunTime <= m_MaxFlapRunTime)
                    {
                        m_PlayerData.state.Value = PlayerData.KuraState.FlapRun;
                        m_CurFlapRunTime += Time.deltaTime;
                    }
                    else
                    {
                        m_PlayerData.state.Value = PlayerData.KuraState.ReadyRun;
                    }
                }
            }
            else
            {
                if (m_RigidBody2d.velocity.x < m_MinFlyVelocity)
                    m_PlayerData.state.Value = PlayerData.KuraState.Fall;
                else if (m_RigidBody2d.velocity.x <= m_MaxFlyVelocity)
                    m_PlayerData.state.Value = PlayerData.KuraState.Fly;
                else
                    m_PlayerData.state.Value = PlayerData.KuraState.Glide;
            }
        }

        if (m_PlayerData.state.Value == PlayerData.KuraState.Stand)
        {

        }
        else if (m_PlayerData.state.Value == PlayerData.KuraState.Fall)
        {

        }
        else if (m_PlayerData.state.Value == PlayerData.KuraState.Run)
        {
            Tuple<string, int, string> feetPlatform = FindFeetPlatform();

            if (feetPlatform.Item1 == "simplePlatform")
            {
                m_RigidBody2d.AddForce(Vector2.right * m_RunForce);
            }
        }
        else if (m_PlayerData.state.Value == PlayerData.KuraState.ReadyRun)
        {
            Tuple<string, int, string> feetPlatform = FindFeetPlatform();

            if (feetPlatform.Item1 == "simplePlatform")
            {
                if (Math.Abs(m_MaxRunVelocity - m_RigidBody2d.velocity.x) > m_ChillThresholdVelocity)
                {
                    if (m_RigidBody2d.velocity.x < m_MaxRunVelocity)
                        m_RigidBody2d.AddForce(Vector2.right * m_ReadyRunForce);
                    else
                        m_RigidBody2d.AddForce(Vector2.left * m_RunBrakeForce);
                }
            }
        }
        else if (m_PlayerData.state.Value == PlayerData.KuraState.FlapRun)
        {
            
        }
        else if (m_PlayerData.state.Value == PlayerData.KuraState.Fly)
        {
            if (Math.Abs(m_MaxFlyVelocity - m_RigidBody2d.velocity.x) > m_ChillThresholdVelocity)
                m_RigidBody2d.AddForce(Vector2.right * m_FlyForce);
        }
        else if (m_PlayerData.state.Value == PlayerData.KuraState.Glide)
        {
            if (Math.Abs(m_MaxFlyVelocity - m_RigidBody2d.velocity.x) > m_ChillThresholdVelocity)
                m_RigidBody2d.AddForce(Vector2.left * m_FlyBrakeForce);
        }
        else
        {
            Debug.Log("No kura state ???");
        }

        for (int i=0; i<m_TouchingPlatforms.Count; i++)
        {
            Debug.Log(i + " " + m_TouchingPlatforms[i].Item1 + " " + m_TouchingPlatforms[i].Item2 + " " + m_TouchingPlatforms[i].Item3);
        }
    }

    private Tuple<string, int, string> FindFeetPlatform()
    {
        // this probably should return all feet platforms, but irrelevant for now

        foreach(Tuple<string, int, string> platform in m_TouchingPlatforms)
        {
            if ((m_GravityDirection == 1 && platform.Item2 == 2) ||
                (m_GravityDirection == -1 && platform.Item2 == 0))
            {
                return platform;
            }
        }

        return null;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsServer) return;

        //Debug.Log(collision.gameObject.tag);
        if (Array.Exists(m_FlipTagList, element => collision.gameObject.CompareTag(element)))
        {
            m_NFlips ++;
            m_NFlips = Math.Min(m_NFlips, m_MaxFlips);
        }

        int dir = FindCollisionDirection(collision);
        m_TouchingPlatforms.Add(new Tuple<string, int, string>(collision.gameObject.tag, dir, collision.gameObject.name));

        if ((m_GravityDirection == 1 && dir == 2) ||
            (m_GravityDirection == -1 && dir == 0))
        {
            m_CurFlapRunTime = 0f;
        }

        foreach (ContactPoint2D contact in collision.contacts)
        {
            Debug.DrawLine(new Vector3(contact.point.x, contact.point.y, transform.position.z), transform.position, Color.green, 2, false);
        }
        //Debug.Break();
    }

    private int FindCollisionDirection(Collision2D collision)
    {
        // Direction is absolute
        // 0 - up, 1 -right, 2 - down, 3 - left
        bool[] directions = { true, true, true, true };

        if (collision.contacts.Length != 2)
        {
            Debug.Log("HELP HELP HELP HELP HELP BAD COLLISION");
            return -1;
        }

        foreach(ContactPoint2D contact in collision.contacts)
        {
            float dx = contact.point.x - transform.position.x;
            float dy = contact.point.y - transform.position.y;

            directions[0] = directions[0] && (dy > 0);
            directions[1] = directions[1] && (dx > 0);
            directions[2] = directions[2] && (dy < 0);
            directions[3] = directions[3] && (dx < 0);
        }

        for (int i = 0; i < 4; i++)
        {
            if (directions[i])
                return i;
        }

        return -1;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (!IsServer) return;

        m_TouchingPlatforms.RemoveAt(m_TouchingPlatforms.FindIndex(e => e.Item3 == collision.gameObject.name));
    }

    private void OnFinishedGameChanged(bool previous, bool current)
    {
        if (IsServer)
        {
            m_RigidBody2d.gravityScale = 0;
            m_RigidBody2d.velocity = Vector2.zero;
            transform.position += new Vector3(UnityEngine.Random.Range(m_RangeTeleportation.x, m_RangeTeleportation.y), 0f, 0f);
        }
    }
}