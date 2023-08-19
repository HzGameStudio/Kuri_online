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

// This class controls kura's movement
// By extension this is the biggest and the shittiest and the badass-est script in the project
public class PlayerControl : NetworkBehaviour
{
    // Boost tracking veriables
    private bool m_IsSpeedBoosted = false;
    private float m_CurSpeedBoostTime = 0;
    private float m_CurSpeedBoostForce = 0;

    // *** Constants
    // All these variables are "constant",
    // but we can't make them actually constant because they assigned in run-time first, then are never changed

    // Physics
    // These variables define kura's behaviour movement in the air
    // Kura's movement on the ground is defined by the type of the platform, later this will be the same for air movement (probably)

    [SerializeField]
    private float m_MinFlyVelocity;

    [SerializeField]
    private float m_MaxFlyVelocity;

    [SerializeField]
    private float m_AbsoluteMaxVelocity;

    // This value is to prevent kura from speeding up, then on next frame slowing down, then speeding up again, etc.,
    // so kura's velocity doesn't change when it's +- <m_ChillThresholdVelocity> from the currently desired value
    [SerializeField]
    private float m_ChillThresholdVelocity;

    [SerializeField]
    private float m_FlyForce;

    [SerializeField]
    private float m_FlyBrakeForce;

    [SerializeField]
    private float m_GravityMultiplier;

    // Objects

    [SerializeField]
    private BoxCollider2D m_BoxCollider2D;

    [SerializeField]
    private Rigidbody2D m_RigidBody2d;

    // square transform, not player transform
    [SerializeField]
    private Transform m_Transform;

    [SerializeField]
    private GameObject m_RedKura;

    [SerializeField]
    private GameObject m_BlueKura;

    private GameData m_GameData;

    private PlayerData m_PlayerData;

    // Logic

    // List of objects that give the player a flip upon contact
    [SerializeField]
    private string[] m_FlipTagList = { "simplePlatform", "player" };

    [SerializeField]
    private int m_MaxFlips = 1;

    // Other

    // Where to teleport kura after finish
    [SerializeField]
    private Vector2 m_FinishTPDistance = new Vector2(2, 10);



    // *** Active
    // These values often change in run-time

    // This indicates for the server when a client pressed the screen, then the server processes it
    private bool m_Request = false;

    private int m_GravityDirection = 1;

    private int m_NFlips = 1;

    // This list stores all objects that kura is currently touching (btw i forgor about other kuras, hz how it works :skull:)
    // Platform tag, platform direction, platform gameObject name
    public List<Tuple<string, int, string>> m_TouchingPlatforms = new List<Tuple<string, int, string>>();

    private float m_CurFlapRunTime = 0f;

    private void Start()
    {
        //Time.timeScale = 0.3f;

        m_PlayerData = GetComponent<PlayerData>();

        m_GameData = GameObject.FindGameObjectWithTag("gameManager").GetComponent<GameData>();

        m_GameData.m_PlayerDataList.Add(gameObject);

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

        if (IsServer)
        {
            transform.position = m_GameData.GetSpawnPosition();
        }

        m_PlayerData.finishedgame.OnValueChanged += OnFinishedGameChanged;
    }

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
        if (!(m_GameData.isGameRunning.Value && !m_PlayerData.finishedgame.Value)) return;

        m_PlayerData.playerRunTime.Value += Time.deltaTime;

        // Process client's request to jump
        if (m_Request)
        {
            m_Request = false;

            // Flip
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
        if(m_PlayerData.finishedgame.Value == false)
        {
            // Request to flip
            if ((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) || Input.GetMouseButtonDown(0))
            {
                UpdateClientPositionServerRpc(true);
            }
        }

        String temp = Math.Floor(m_PlayerData.playerRunTime.Value / 60f).ToString() + ":" + Math.Floor(m_PlayerData.playerRunTime.Value).ToString() + "." + Math.Floor(m_PlayerData.playerRunTime.Value * 10) % 10 + Math.Floor(m_PlayerData.playerRunTime.Value * 100) % 10;
        m_GameData.playerRunTimeText.text = temp;

        Debug.DrawLine(transform.position, new Vector3(transform.position.x + m_RigidBody2d.velocity.x, transform.position.y, transform.position.z), Color.red, 1 / 300f);
    }

    // [ServerRpc] is used to change a <NetworkVariable> from client-side
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
        if (!(m_GameData.isGameRunning.Value && !GetComponent<PlayerData>().finishedgame.Value)) return;

        // This is where the pizdec starts :skull:

        // First, we decide whether kura is standing on some platform and whether kura is bumped into a wall

        // Lambda majic
        // Translated into words this means:
        // "Does there exist Any platform that satisfies these condition:
        //  we need a <platform> that (m_GravityDirection == 1 && platform.Item2 == 2) or (m_GravityDirection == -1 && platform.Item2 == 0)"
        bool onFloor = m_TouchingPlatforms.Any(platform => (m_GravityDirection == 1 && platform.Item2 == 2) ||
                                                           (m_GravityDirection == -1 && platform.Item2 == 0));

        // Translated into words this means:
        // "Does there exist Any platform that satisfies these condition:
        //  we need a <platform> that platform.Item2 == 1
        bool kissWall = m_TouchingPlatforms.Any(platform => platform.Item2 == 1);

        // This next whole chunk of code decides what state kura is currently in
        // kura's state dictates how kura's physics will updated later

        // If kura is bumped into a wall, there can be only 2 states, <Stand> and <Fall>
        if (kissWall)
        {
            if (onFloor)
                m_PlayerData.state.Value = PlayerData.KuraState.Stand;
            else
                m_PlayerData.state.Value = PlayerData.KuraState.Fall;
        }
        else
        {
            // Code to boost kura if it is <m_IsSpeedBoosted>
            if (m_IsSpeedBoosted && m_CurSpeedBoostTime > 0)
            {
                m_CurSpeedBoostTime -= Time.deltaTime;
                m_RigidBody2d.AddForce(Vector2.right * m_CurSpeedBoostForce);
                Debug.Log("isBOOOSTED");
            }
            else
            {
                m_IsSpeedBoosted = false;
                m_CurSpeedBoostTime = 0;
            }


            if (onFloor)
            {
                // This finds the platform that kura is currently standing on and returns it's tag, direction relative to kura, and name on the scene
                Tuple<string, int, string> feetPlatform = FindFeetPlatform();

                // This finds the data values asociated to the <feetPlatform>
                PlatformScreaptebleObject platformData = GameObject.Find(feetPlatform.Item3).GetComponent<PlatformBasicScript>().platformData;

                // note for me: m_MinFlyVelocity probably should be an atribute of a platform, ask yarik
                if (m_RigidBody2d.velocity.x < m_MinFlyVelocity)
                {
                    m_PlayerData.state.Value = PlayerData.KuraState.Run;
                }
                else if (m_RigidBody2d.velocity.x <= platformData.m_MaxRunVelocity)
                {
                    m_PlayerData.state.Value = PlayerData.KuraState.ReadyRun;
                }
                else
                {
                    // Flap run is a mechanic to allow to easily chain flying:
                    // eg. you run on top platform on max running speed, then flip, gain some more speed in the air,
                    // land on the bottom platform, then you have a little grace period of <m_MaxFlapRunTime> when you don't lose speed,
                    // so you can flip and maintain all you speed
                    // by effectively chaining flips you can more effectively gain speed in the air
                    // (maybe stupid idea but who knows :P)
                    if (m_CurFlapRunTime <= platformData.m_MaxFlapRunTime)
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

        // Now that we've decided what state kura is in, we apply forces and stuff to it

        if (m_PlayerData.state.Value == PlayerData.KuraState.Stand)
        {

        }
        else if (m_PlayerData.state.Value == PlayerData.KuraState.Fall)
        {

        }
        else if (m_PlayerData.state.Value == PlayerData.KuraState.Run)
        {
            Tuple<string, int, string> feetPlatform = FindFeetPlatform();

            PlatformScreaptebleObject platformData = GameObject.Find(feetPlatform.Item3).GetComponent<PlatformBasicScript>().platformData;

            if (feetPlatform.Item1 == "simplePlatform")
            {
                m_RigidBody2d.AddForce(Vector2.right * platformData.m_RunForce);
            }
        }
        else if (m_PlayerData.state.Value == PlayerData.KuraState.ReadyRun)
        {
            Tuple<string, int, string> feetPlatform = FindFeetPlatform();

            PlatformScreaptebleObject platformData = GameObject.Find(feetPlatform.Item3).GetComponent<PlatformBasicScript>().platformData;

            if (feetPlatform.Item1 == "simplePlatform")
            {
                // Implementaion of m_ChillThresholdVelocity (see line 38)
                if (Math.Abs(platformData.m_MaxRunVelocity - m_RigidBody2d.velocity.x) > m_ChillThresholdVelocity)
                {
                    if (m_RigidBody2d.velocity.x < platformData.m_MaxRunVelocity)
                        m_RigidBody2d.AddForce(Vector2.right * platformData.m_ReadyRunForce);
                    else
                        m_RigidBody2d.AddForce(Vector2.left * platformData.m_RunBrakeForce);
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

    // Function that finds the feet platform, it just returns the first platform that is below kura's legs
    // (relative to current kura gravity)
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

        // Check if the platform hit is eligible to give a flip
        if (Array.Exists(m_FlipTagList, element => collision.gameObject.CompareTag(element)))
        {
            m_NFlips ++;
            m_NFlips = Math.Min(m_NFlips, m_MaxFlips);
        }

        // Adds the encountered object to the list
        int dir = FindCollisionDirection(collision);
        m_TouchingPlatforms.Add(new Tuple<string, int, string>(collision.gameObject.tag, dir, collision.gameObject.name));

        // Resets <m_CurFlapRunTime>
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

    // Just finds which direction the hit object is relative to kura (not relative to kura's direction, aka. absolute)
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

        // Remove the object kura is not touching anymore from the list
        m_TouchingPlatforms.RemoveAt(m_TouchingPlatforms.FindIndex(e => e.Item3 == collision.gameObject.name));
    }

    // This is called by a boost when encountered (see SpeedBoostScript.cs (15))
    public void Boost(SpeedBoostScriptableObject speedBoostData)
    {
        m_IsSpeedBoosted = true;
        m_CurSpeedBoostTime = speedBoostData.boostTime;
        m_CurSpeedBoostForce = speedBoostData.boostForce;
    }

    // Stop kura when it finishes
    private void OnFinishedGameChanged(bool previous, bool current)
    {
        if (IsServer)
        {
            m_RigidBody2d.gravityScale = 0;
            m_RigidBody2d.velocity = Vector2.zero;
            transform.position += new Vector3(UnityEngine.Random.Range(m_FinishTPDistance.x, m_FinishTPDistance.y), 0f, 0f);
        }
    }
}