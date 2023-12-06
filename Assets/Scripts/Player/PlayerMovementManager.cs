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
using UnityEngine.UIElements;

// Class to controls kura's movement
public class PlayerMovementManager : NetworkBehaviour
{
    public struct KuraTransfromData : INetworkSerializable
    {
        public KuraTransfromData (Vector3 positionIn,
                           int gravityDirectionIn,
                           float gravityMultiplierIn,
                           Vector3 velocityIn)
        {
            position = positionIn;
            gravityDirection = gravityDirectionIn;
            gravityMultiplier = gravityMultiplierIn;
            velocity = velocityIn;
        }

        public Vector3 position;
        public int gravityDirection;
        public float gravityMultiplier;
        public Vector3 velocity;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref position);
            serializer.SerializeValue(ref gravityDirection);
            serializer.SerializeValue(ref gravityMultiplier);
            serializer.SerializeValue(ref velocity);
        }
    }

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
    private Transform m_SkinTransform;

    [SerializeField]
    private GameObject m_RedKura;

    [SerializeField]
    private GameObject m_BlueKura;

    private PlayerMain m_PlayerMain;

    private PlayerUIManager m_PlayerUIManager;

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

    [DoNotSerialize, HideInInspector]
    private NetworkVariable<KuraTransfromData> transformFromClient = new NetworkVariable<KuraTransfromData>();

    private void Start()
    {
        //Time.timeScale = 0.5f;

        m_PlayerMain = GetComponent<PlayerMain>();
        m_PlayerUIManager = GetComponent<PlayerUIManager>();

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

        transformFromClient.OnValueChanged += OnKuraTransformDataFromClientChanged;
    }

    private void Update()
    {
        if (IsClient && IsOwner)
        {
            TakeInput();

            ProcessLocalPlayerInput();

            Debug.DrawLine(transform.position, new Vector3(transform.position.x + m_RigidBody2d.velocity.x, transform.position.y, transform.position.z), Color.red, 1 / 300f);
        }
    }

    private void TakeInput()
    {
        if (!(m_PlayerMain.localData.gameMode == KuraGameMode.ClasicMode))
            return;

        if (m_PlayerMain.localData.finishedGame)
            return;

        // Request to flip
        if ((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) || Input.GetMouseButtonDown(0))
        {
            m_Request = true;
        }
    }

    private void ProcessLocalPlayerInput()
    {
        if (!(m_PlayerMain.localData.gameMode == KuraGameMode.ClasicMode))
            return;

        if (m_PlayerMain.localData.finishedGame)
            return;

        if (!m_Request)
            return;

        // Process client's request to jump
        m_Request = false;

        // Flip
        if (m_NFlips <= 0)
            return;


        m_NFlips--;

        m_GravityDirection *= -1;


        Flip(m_GravityDirection);
    }

    private void Flip(int gravityDirection)
    {
        m_RigidBody2d.gravityScale = gravityDirection * m_GravityMultiplier;

        // change kura skin orientation
        m_SkinTransform.localScale = new Vector3(m_SkinTransform.localScale.x, Math.Abs(m_SkinTransform.localScale.y) * gravityDirection, m_SkinTransform.localScale.z);
    }

    private void TakePeriodicDamageFromPlatmorm()
    {
        PlatformBasicScript platformData;
        foreach(Tuple<GameObject, int> platform in m_TouchingObjects)
        {
            // check if it's a platform (doesn't work right now ?)
            if (false)
                return;

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
                m_PlayerMain.Damage(platformData.platformData.damage);
                platformData.isDamageTimerRuning = true;
            }
        }
    }

    private void FixedUpdate()
    {
        if (IsClient && IsOwner)
        {
            ProcessMovement();

            TakePeriodicDamageFromPlatmorm();

            UpdatePositionOnServerRPC(new KuraTransfromData(transform.position, m_GravityDirection, m_GravityMultiplier, m_RigidBody2d.velocity));
        }
    }

    private void ProcessMovement()
    {
        if (m_PlayerMain.localData.gameMode == KuraGameMode.ClasicMode)
        { 
            if (m_PlayerMain.localData.finishedGame)
                return;

            KuraState state;

            bool onFloor = m_TouchingObjects.Any(platform => (m_GravityDirection == 1 && platform.Item2 == 2) ||
                                                               (m_GravityDirection == -1 && platform.Item2 == 0));

            bool kissWall = m_TouchingObjects.Any(platform => platform.Item2 == 1);

            if (kissWall)
            {
                if (onFloor)
                    state = KuraState.Stand;
                else
                    state = KuraState.Fall;
            }
            else
            {
                if (m_IsSpeedBoosted && m_CurSpeedBoostTime > 0)
                {
                    m_CurSpeedBoostTime -= Time.fixedDeltaTime;
                    m_RigidBody2d.totalForce = new Vector2(m_CurSpeedBoostForce, m_RigidBody2d.totalForce.y);
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
                        state = KuraState.Run;
                    }
                    else if (m_RigidBody2d.velocity.x <= platformData.MaxRunVelocity)
                    {
                        state = KuraState.ReadyRun;
                    }
                    else
                    {
                        if (m_CurFlapRunTime <= platformData.MaxFlapRunTime)
                        {
                            state = KuraState.FlapRun;
                            m_CurFlapRunTime += Time.fixedDeltaTime;
                        }
                        else
                        {
                            state = KuraState.ReadyRun;
                        }
                    }
                }
                else
                {
                    if (m_RigidBody2d.velocity.x < m_MinFlyVelocity)
                        state = KuraState.Fall;
                    else if (m_RigidBody2d.velocity.x <= m_MaxFlyVelocity)
                        state = KuraState.Fly;
                    else
                        state = KuraState.Glide;
                }
            }

            if (state == KuraState.Stand)
            {

            }
            else if (state == KuraState.Fall)
            {

            }
            else if (state == KuraState.Run)
            {
                Tuple<GameObject, int> feetPlatform = FindFeetPlatform();

                PlatformScreaptebleObject platformData = feetPlatform.Item1.GetComponent<PlatformBasicScript>().platformData;

                if (feetPlatform.Item1.tag == "simplePlatform")
                {
                    m_RigidBody2d.totalForce = new Vector2(platformData.RunForce, m_RigidBody2d.totalForce.y);
                }
            }
            else if (state == KuraState.ReadyRun)
            {
                Tuple<GameObject, int> feetPlatform = FindFeetPlatform();

                PlatformScreaptebleObject platformData = feetPlatform.Item1.GetComponent<PlatformBasicScript>().platformData;

                if (feetPlatform.Item1.tag == "simplePlatform")
                {
                    if (Math.Abs(platformData.MaxRunVelocity - m_RigidBody2d.velocity.x) > m_ChillThresholdVelocity)
                    {
                        if (m_RigidBody2d.velocity.x < platformData.MaxRunVelocity)
                            m_RigidBody2d.totalForce = new Vector2(platformData.ReadyRunForce, m_RigidBody2d.totalForce.y);
                        else
                            m_RigidBody2d.totalForce = new Vector2(-platformData.RunBrakeForce, m_RigidBody2d.totalForce.y);
                    }
                }
            }
            else if (state == KuraState.FlapRun)
            {

            }
            else if (state == KuraState.Fly)
            {
                if (Math.Abs(m_MaxFlyVelocity - m_RigidBody2d.velocity.x) > m_ChillThresholdVelocity)
                    m_RigidBody2d.totalForce = new Vector2(m_FlyForce, m_RigidBody2d.totalForce.y);
            }
            else if (state == KuraState.Glide)
            {
                if (Math.Abs(m_MaxFlyVelocity - m_RigidBody2d.velocity.x) > m_ChillThresholdVelocity)
                    m_RigidBody2d.totalForce = new Vector2(-m_FlyBrakeForce, m_RigidBody2d.totalForce.y);
            }
            else
            {
                Debug.Log("No kura state ???");
            }

            m_PlayerMain.localData.state = state;
        }
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
        if (!(IsClient && IsOwner))
            return;

        // Check if the platform hit is eligible to give a flip
        if (Array.Exists(m_FlipTagList, element => collision.gameObject.CompareTag(element)))
        {
            m_NFlips++;
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
        if (!(IsClient && IsOwner))
            return;

        // Remove the object kura is not touching anymore from the list
        m_TouchingObjects.RemoveAt(m_TouchingObjects.FindIndex(e => e.Item1 == collision.gameObject));
    }

    public void Respawn()
    {
        transform.position = m_PlayerMain.localData.spawnData.position;
        m_RigidBody2d.velocity = m_PlayerMain.localData.spawnData.velocity;
        m_GravityDirection = m_PlayerMain.localData.spawnData.gravityDirection;
        m_GravityMultiplier = m_PlayerMain.localData.spawnData.gravityMultiplier;
        Flip(m_GravityDirection); 
    }

    public void Boost(SpeedBoostScriptableObject speedBoostData)
    {
        m_IsSpeedBoosted = true;
        m_CurSpeedBoostTime = speedBoostData.boostTime;
        m_CurSpeedBoostForce = speedBoostData.boostForce;
    }

    // Stop kura when it finishes
    public void Finish()
    {
        if (!(IsClient && IsOwner))
            return;

        Debug.Log("Movement script");

        m_GravityMultiplier = 0.0f;
        m_RigidBody2d.gravityScale = m_GravityMultiplier;

        m_RigidBody2d.velocity = Vector2.zero;
        transform.position += new Vector3(UnityEngine.Random.Range(m_FinishTPDistance.x, m_FinishTPDistance.y), 0f, 0f);
    }

    void ApplyTransformLocally(KuraTransfromData localTransformData)
    {
        transform.position = localTransformData.position;
        m_RigidBody2d.velocity = localTransformData.velocity;

        m_GravityDirection = localTransformData.gravityDirection;
        m_GravityMultiplier = localTransformData.gravityMultiplier;
        m_RigidBody2d.gravityScale = m_GravityDirection * m_GravityMultiplier;


        Flip(localTransformData.gravityDirection);
    }

    [ServerRpc]
    void UpdatePositionOnServerRPC(KuraTransfromData localTransformData)
    {
        transformFromClient.Value = localTransformData;

        if (IsOwner)
            return;

        ApplyTransformLocally(localTransformData);
    }

    void OnKuraTransformDataFromClientChanged(KuraTransfromData previous, KuraTransfromData current)
    {
        if (IsOwner || IsServer)
            return;

        // move the following lines to function (to not duplicate code)

        ApplyTransformLocally(current);
    }

    public Vector2 GetVelocity()
    {
        return m_RigidBody2d.velocity;
    }
}