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
using System.Runtime.CompilerServices;
using UnityEngine.PlayerLoop;

// Class to controls kura's movement
public class PlayerControl : NetworkBehaviour
{
    // Boost tracking veriables
    private bool m_IsSpeedBoosted = false;
    private float m_CurSpeedBoostTime = 0;
    private float m_CurSpeedBoostForce = 0;

    // *** Constants

    // Physics

    [SerializeField]
    private float m_MinFlyVelocity;

    [SerializeField]
    private float m_MaxFlyVelocity;

    [SerializeField]
    private float m_AbsoluteMaxVelocity;

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

    private PlayerData m_PlayerData;

    private PlayerUIManager m_PlayerUIManagre;

    // Logic

    // List of objects that give the player a flip upon contact
    [SerializeField]
    private string[] m_FlipTagList = {"simplePlatform"};

    [SerializeField]
    private int m_MaxFlips = 1;

    // Other

    // Where to teleport kura after finish
    [SerializeField]
    private Vector2 m_FinishTPDistance = new Vector2(2, 10);



    // *** Active

    // This indicates for the server when a client pressed the screen, then the server processes it
    private bool m_Request = false;

    private int m_GravityDirection = 1;

    private int m_NFlips = 1;

    // Platform tag, platform direction, platform gameObject name
    public List<Tuple<GameObject, int>> m_TouchingObjects = new List<Tuple<GameObject, int>>();

    private float m_CurFlapRunTime = 0f;

    public NetworkVariable<Vector3> m_KuraPositionFromClient = new NetworkVariable<Vector3>();
    public NetworkVariable<int> m_KuraGravityDirectionFromClient = new NetworkVariable<int>();
    public NetworkVariable<Vector3> m_KuraVelocityFromClient = new NetworkVariable<Vector3>();

    private void Start()
    {
        //Time.timeScale = 0.5f;

        m_PlayerData = GetComponent<PlayerData>();
        m_PlayerUIManagre = GetComponent<PlayerUIManager>();

        MainManager.Instance.playerDataList.Add(m_PlayerData);

        //this makes you see yourself as a blue kura
        //while other players are red 
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

        m_PlayerData.finishedGame.OnValueChanged += OnFinishedGameChanged;

        m_KuraPositionFromClient.OnValueChanged += OnKuraPositionFromClientChanged;
        m_KuraGravityDirectionFromClient.OnValueChanged += OnKuraGravityFromClientChanged;
        m_KuraVelocityFromClient.OnValueChanged += OnKuraVelocityFromClientChanged;
    }

    private void Update()
    {
        if (IsClient && IsOwner)
        {
            TakeInput();

            ProcessLocalPlayerInput();
        }
    }

    // separate process input and move 
    /*
      m_GravityDirection *= -1;
                    m_RigidBody2d.gravityScale = m_GravityDirection * m_GravityMultiplier;

                    // change kura skin orientation
                    m_Transform.localScale = new Vector3(m_Transform.localScale.x, m_Transform.localScale.y * -1, m_Transform.localScale.z);
    */
    private void ProcessLocalPlayerInput()
    {
        if(m_PlayerData.gameMode.Value == PlayerData.KuraGameMode.ClasicMode)
        {
            if (m_PlayerData.finishedGame.Value)
                return;

            //CheckHealth();

            // Process client's request to jump
            if (m_Request)
            {
                m_Request = false;

                // Flip
                if (m_NFlips > 0)
                {
                    m_GravityDirection *= -1;
                    m_RigidBody2d.gravityScale = m_GravityDirection * m_GravityMultiplier;

                    // change kura skin orientation
                    m_Transform.localScale = new Vector3(m_Transform.localScale.x, Math.Abs(m_Transform.localScale.y) * m_GravityDirection, m_Transform.localScale.z);

                    m_NFlips--;
                }
            }
        }
    }

    void OnKuraPositionFromClientChanged(Vector3 previous, Vector3 current)
    {
        if (!(IsOwner || IsServer))
        {
            transform.position = current;
        }
    }

    void OnKuraGravityFromClientChanged(int previous, int current)
    {
        if (!(IsOwner || IsServer))
        {
            m_RigidBody2d.gravityScale = current * m_GravityMultiplier;

            // change kura skin orientation
            m_Transform.localScale = new Vector3(m_Transform.localScale.x, Math.Abs(m_Transform.localScale.y) * current, m_Transform.localScale.z);
        }
    }

    void OnKuraVelocityFromClientChanged(Vector3 previous, Vector3 current)
    {
        if (!(IsOwner || IsServer))
        {
            m_RigidBody2d.velocity = current;
        }
    }

    private void TakeDamageFromPlatmorm()
    {
        PlatformBasicScript platformData;
        foreach(Tuple<GameObject, int> platform in m_TouchingObjects)
        {
            if(true)
            {
                platformData = platform.Item1.GetComponent<PlatformBasicScript>();
                if(platformData.isDamageTimerRuning)
                {
                    platformData.currentTime += Time.fixedDeltaTime;
                    if(platformData.currentTime >= platformData.platformData.deltaTimeDamage)
                    {
                        platformData.isDamageTimerRuning = false;
                        platformData.currentTime = 0;
                    }
                }
                else
                {
                    Damage(platformData.platformData.instanteDamage);
                    platformData.isDamageTimerRuning = true;
                }
            }
        }
    }

    public void Damage(float damage)
    {
        ChangePlayerHealthServerRPC(m_PlayerData.playerHealth.Value + damage);
        CheckHealth();
    }

    private void CheckHealth()
    {
        if(m_PlayerData.playerHealth.Value < 0)
        {
            //Dead
            Respawn();
        }
    }

    private void Respawn()
    { 
        transform.position = m_PlayerData.spawnPosition.Value;
        ChangePlayerHealthServerRPC(PlayerData.playerStartHealth);
    }

    [ServerRpc]
    void ChangePlayerHealthServerRPC(float health)
    {
        m_PlayerData.playerHealth.Value = health;
    }

    private void TakeInput()
    {
        if (m_PlayerData.gameMode.Value == PlayerData.KuraGameMode.ClasicMode)
        {
            if (m_PlayerData.finishedGame.Value == false)
            {
                // Request to flip
                if ((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) || Input.GetMouseButtonDown(0))
                {
                    m_Request = true;
                }
            }
        }

        Debug.DrawLine(transform.position, new Vector3(transform.position.x + m_RigidBody2d.velocity.x, transform.position.y, transform.position.z), Color.red, 1 / 300f);
    }

    private void FixedUpdate()
    {
        if (IsClient && IsOwner)
        {
            FixedUpdateClient();

            UpdatePositionOnServerRPC(transform.position, m_GravityDirection, m_RigidBody2d.velocity);
        }
    }

    [ServerRpc]
    void UpdatePositionOnServerRPC(Vector3 pos, int gravity_direction, Vector3 velocity)
    {
        m_KuraPositionFromClient.Value = pos;
        m_KuraGravityDirectionFromClient.Value = gravity_direction;
        m_KuraVelocityFromClient.Value = velocity;

        transform.position = pos;
        m_RigidBody2d.gravityScale = gravity_direction * m_GravityMultiplier;

        // change kura skin orientation
        m_Transform.localScale = new Vector3(m_Transform.localScale.x, Math.Abs(m_Transform.localScale.y) * gravity_direction, m_Transform.localScale.z);

        m_RigidBody2d.velocity = velocity;
    }

    private void FixedUpdateClient()
    {
        if (m_PlayerData.gameMode.Value == PlayerData.KuraGameMode.ClasicMode)
        { 
            if (m_PlayerData.finishedGame.Value) return;

            PlayerData.KuraState state;

            TakeDamageFromPlatmorm();

            bool onFloor = m_TouchingObjects.Any(platform => (m_GravityDirection == 1 && platform.Item2 == 2) ||
                                                               (m_GravityDirection == -1 && platform.Item2 == 0));

            bool kissWall = m_TouchingObjects.Any(platform => platform.Item2 == 1);

            if (kissWall)
            {
                if (onFloor)
                    state = PlayerData.KuraState.Stand;
                else
                    state = PlayerData.KuraState.Fall;
            }
            else
            {
                if (m_IsSpeedBoosted && m_CurSpeedBoostTime > 0)
                {
                    m_CurSpeedBoostTime -= Time.fixedDeltaTime;
                    m_RigidBody2d.AddForce(Vector2.right * m_CurSpeedBoostForce);
                }
                else
                {
                    m_IsSpeedBoosted = false;
                    m_CurSpeedBoostTime = 0;
                }


                if (onFloor)
                {
                    Tuple<GameObject, int> feetPlatform = FindFeetPlatform();

                    PlatformScreaptebleObject platformData = feetPlatform.Item1.GetComponent<PlatformBasicScript>().platformData;

                    if (m_RigidBody2d.velocity.x < m_MinFlyVelocity)
                    {
                        state = PlayerData.KuraState.Run;
                    }
                    else if (m_RigidBody2d.velocity.x <= platformData.MaxRunVelocity)
                    {
                        state = PlayerData.KuraState.ReadyRun;
                    }
                    else
                    {
                        if (m_CurFlapRunTime <= platformData.MaxFlapRunTime)
                        {
                            state = PlayerData.KuraState.FlapRun;
                            m_CurFlapRunTime += Time.fixedDeltaTime;
                        }
                        else
                        {
                            state = PlayerData.KuraState.ReadyRun;
                        }
                    }
                }
                else
                {
                    if (m_RigidBody2d.velocity.x < m_MinFlyVelocity)
                        state = PlayerData.KuraState.Fall;
                    else if (m_RigidBody2d.velocity.x <= m_MaxFlyVelocity)
                        state = PlayerData.KuraState.Fly;
                    else
                        state = PlayerData.KuraState.Glide;
                }
            }

            if (state == PlayerData.KuraState.Stand)
            {

            }
            else if (state == PlayerData.KuraState.Fall)
            {

            }
            else if (state == PlayerData.KuraState.Run)
            {
                Tuple<GameObject, int> feetPlatform = FindFeetPlatform();

                PlatformScreaptebleObject platformData = feetPlatform.Item1.GetComponent<PlatformBasicScript>().platformData;

                if (feetPlatform.Item1.tag == "simplePlatform")
                {
                    m_RigidBody2d.AddForce(Vector2.right * platformData.RunForce);
                }
            }
            else if (state == PlayerData.KuraState.ReadyRun)
            {
                Tuple<GameObject, int> feetPlatform = FindFeetPlatform();

                PlatformScreaptebleObject platformData = feetPlatform.Item1.GetComponent<PlatformBasicScript>().platformData;

                if (feetPlatform.Item1.tag == "simplePlatform")
                {
                    if (Math.Abs(platformData.MaxRunVelocity - m_RigidBody2d.velocity.x) > m_ChillThresholdVelocity)
                    {
                        if (m_RigidBody2d.velocity.x < platformData.MaxRunVelocity)
                            m_RigidBody2d.AddForce(Vector2.right * platformData.ReadyRunForce);
                        else
                            m_RigidBody2d.AddForce(Vector2.left * platformData.RunBrakeForce);
                    }
                }
            }
            else if (state == PlayerData.KuraState.FlapRun)
            {

            }
            else if (state == PlayerData.KuraState.Fly)
            {
                if (Math.Abs(m_MaxFlyVelocity - m_RigidBody2d.velocity.x) > m_ChillThresholdVelocity)
                    m_RigidBody2d.AddForce(Vector2.right * m_FlyForce);
            }
            else if (state == PlayerData.KuraState.Glide)
            {
                if (Math.Abs(m_MaxFlyVelocity - m_RigidBody2d.velocity.x) > m_ChillThresholdVelocity)
                    m_RigidBody2d.AddForce(Vector2.left * m_FlyBrakeForce);
            }
            else
            {
                Debug.Log("No kura state ???");
            }

            UpdateKuraStateOnServerRPC(state);
        }
    }

    [ServerRpc]
    void UpdateKuraStateOnServerRPC(PlayerData.KuraState state)
    {
        m_PlayerData.state.Value = state;
    }

    // Function that finds the feet platform, returns the first platform that is below kura's legs
    // (relative to current kura gravity)
    private Tuple<GameObject, int> FindFeetPlatform()
    {
        // NOTE: this probably should return all feet platforms, but irrelevant for now

        foreach(Tuple<GameObject, int> platform in m_TouchingObjects)
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
        if (!(IsClient && IsOwner)) return;

        // Check if the platform hit is eligible to give a flip
        if (Array.Exists(m_FlipTagList, element => collision.gameObject.CompareTag(element)))
        {
            m_NFlips ++;
            m_NFlips = Math.Min(m_NFlips, m_MaxFlips);
        }

        // Adds the encountered object to the list
        int dir = FindCollisionDirection(collision);
        m_TouchingObjects.Add(new Tuple<GameObject, int>(collision.gameObject, dir));

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
    }

    // Just finds which direction the hit object is relative to kura (not relative to kura's direction, aka. absolute)
    private int FindCollisionDirection(Collision2D collision)
    {
        // Direction is absolute
        // 0 - up, 1 -right, 2 - down, 3 - left
        List<(double, int)> directions = new List<(double, int)>();

        if (collision.contacts.Length != 2)
        {
            Debug.Log("HELP HELP HELP HELP HELP BAD COLLISION");
            return -1;
        }

        double dx = collision.contacts[0].point.x - transform.position.x + collision.contacts[1].point.x - transform.position.x;
        double dy = collision.contacts[0].point.y - transform.position.y + collision.contacts[1].point.y - transform.position.y;

        directions.Add((dy, 0));
        directions.Add((dx, 1));
        directions.Add((-dy, 2));
        directions.Add((-dx, 3));

        directions.Sort(delegate ((double, int) a, (double, int) b) { return b.Item1.CompareTo(a.Item1); });

        return directions[0].Item2;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (!(IsClient && IsOwner)) return;

        // Remove the object kura is not touching anymore from the list
        m_TouchingObjects.RemoveAt(m_TouchingObjects.FindIndex(e => e.Item1 == collision.gameObject));
    }

    public void Boost(SpeedBoostScriptableObject speedBoostData)
    {
        m_IsSpeedBoosted = true;
        m_CurSpeedBoostTime = speedBoostData.boostTime;
        m_CurSpeedBoostForce = speedBoostData.boostForce;
    }

    // Stop kura when it finishes
    private void OnFinishedGameChanged(bool previous, bool current)
    {
        if ((IsClient && IsOwner))
        {
            m_RigidBody2d.gravityScale = 0;
            m_RigidBody2d.velocity = Vector2.zero;
            transform.position += new Vector3(UnityEngine.Random.Range(m_FinishTPDistance.x, m_FinishTPDistance.y), 0f, 0f);
        }
    }
}