using System;
using Unity.Netcode;
using UnityEngine;

public enum KuraState
{
    None,
    //Kissing a wall, ground
    Stand,
    //No speed, air
    Fall,
    //No speed, ground
    Run,
    //Normal speed, ground
    ReadyRun,
    //Normal speed, air
    Fly,
    //Too much speed, air
    Glide,
    //Boosted
    Boosted
}

public struct KuraTransfromData : INetworkSerializable
{
    public Vector3 position;
    public Vector3 velocity;
    public int gravityDirection;
    public float gravityMultiplier;
    public int nFlips;

    public KuraTransfromData(Vector3 positionIn,
                       Vector3 velocityIn,
                       int gravityDirectionIn,
                       float gravityMultiplierIn,
                       int nFilpsIn)
    {
        position = positionIn;
        velocity = velocityIn;
        gravityDirection = gravityDirectionIn;
        gravityMultiplier = gravityMultiplierIn;
        nFlips = nFilpsIn;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref position);
        serializer.SerializeValue(ref velocity);
        serializer.SerializeValue(ref gravityDirection);
        serializer.SerializeValue(ref gravityMultiplier);
        serializer.SerializeValue(ref nFlips);
    }
}

// Class to controls kura's movement
public abstract class PlayerMovementBase
{
    #region Variables - should be in environment data
    private readonly float m_MinFlyVelocity = -1;
    private readonly float m_MaxFlyVelocity = 24;
    private readonly float m_AbsoluteMaxVelocity = 40;
    private readonly float m_ChillThresholdVelocity = 1;
    private readonly float m_FlyForce = 5;
    private readonly float m_FlyBrakeForce = 1;
    #endregion

    #region Object pointers
    // square transform, not player transform
    private readonly Transform m_SkinTransform;
    private readonly BoxCollider2D m_BoxCollider2D;
    protected readonly Rigidbody2D m_RigidBody2D;

    private PlayerGeneralBase m_GeneralBase;
    private PlayerInteractionBase m_InteractionBase;

    private readonly GameObject m_Kura; 
    #endregion

    private const int m_MaxFlips = 1;

    // Where to teleport kura after finish
    private readonly Vector2 m_FinishTPDistance = new (2, 10);

    #region Boost tracking variables
    private float m_CurSpeedBoostTime = 0;
    private float m_CurSpeedBoostForce = 0;
    #endregion

    private int m_GravityDirection = 1;
    private float m_GravityMultiplier = 2f;
    private int m_NFlips = 1;

    protected PlayerMovementBase(GameObject kura, Transform skinTransform)
    {
        //Time.timeScale = 0.5f;

        m_Kura = kura;

        m_BoxCollider2D = kura.GetComponent<BoxCollider2D>();
        m_RigidBody2D = kura.GetComponent<Rigidbody2D>();

        m_SkinTransform = skinTransform;
    }

    public void ConnectComponents(PlayerGeneralBase generalBase, PlayerInteractionBase interactionBase)
    {
        m_GeneralBase = generalBase;
        m_InteractionBase = interactionBase;
    }

    public void ProcessInput()
    {
        if (!(m_GeneralBase.LocalData.gameMode == KuraGameMode.ClasicMode))
            return;

        if (m_GeneralBase.LocalData.finishedGame)
            return;

        // Flip
        if (m_NFlips <= 0)
            return;

        m_NFlips--;

        m_GravityDirection *= -1;

        Flip(m_GravityDirection);
    }

    private void Flip(int gravityDirection)
    {
        m_RigidBody2D.gravityScale = gravityDirection * m_GravityMultiplier;

        // change kura skin orientation
        m_SkinTransform.localScale = new Vector3(m_SkinTransform.localScale.x, Math.Abs(m_SkinTransform.localScale.y) * gravityDirection, m_SkinTransform.localScale.z);
    }

    public KuraState GetNewKuraState()
    {
        if (m_GeneralBase.LocalData.finishedGame)
            return m_GeneralBase.LocalData.state;

        if (m_GeneralBase.LocalData.gameMode != KuraGameMode.ClasicMode)
            return m_GeneralBase.LocalData.state;

        if (m_GeneralBase.LocalData.state == KuraState.Boosted)
            return KuraState.Boosted;

        bool onFloor = m_InteractionBase.IsOnFloor();

        bool kissWall = m_InteractionBase.IsKissWall();

        if (kissWall)
        {
            if (onFloor)
                return KuraState.Stand;

            return KuraState.Fall;
        }

        if (onFloor)
        {
            Tuple<GameObject, int> feetPlatform = m_InteractionBase.GiveFeetPlatform();

            PlatformScreaptebleObject platformData = feetPlatform.Item1.GetComponent<PlatformBasicScript>().platformData;

            if (m_RigidBody2D.velocity.x < m_MinFlyVelocity)
                return KuraState.Run;
            else if (m_RigidBody2D.velocity.x <= platformData.MaxRunVelocity)
                return KuraState.ReadyRun;
            else
                return KuraState.ReadyRun;
        }

        if (m_RigidBody2D.velocity.x < m_MinFlyVelocity)
            return KuraState.Fall;
        else if (m_RigidBody2D.velocity.x <= m_MaxFlyVelocity)
            return KuraState.Fly;
        else
            return KuraState.Glide;
    }

    public void ProcessMovement()
    {
        if (m_GeneralBase.LocalData.finishedGame)
            return;

        KuraState state = m_GeneralBase.LocalData.state;

        if (state == KuraState.Stand)
            return;

        if (state == KuraState.Fall)
            return;

        if (state == KuraState.Run)
        {
            Tuple<GameObject, int> feetPlatform = m_InteractionBase.GiveFeetPlatform();
            PlatformScreaptebleObject platformData = feetPlatform.Item1.GetComponent<PlatformBasicScript>().platformData;

            if (feetPlatform.Item1.tag.CompareTo("simplePlatform") == 0)
            {
                m_RigidBody2D.totalForce = new Vector2(platformData.RunForce, m_RigidBody2D.totalForce.y);
            }

            return;
        }

        if (state == KuraState.ReadyRun)
        {
            Tuple<GameObject, int> feetPlatform = m_InteractionBase.GiveFeetPlatform();
            PlatformScreaptebleObject platformData = feetPlatform.Item1.GetComponent<PlatformBasicScript>().platformData;

            if (feetPlatform.Item1.tag.CompareTo("simplePlatform") == 0)
            {
                if (Math.Abs(platformData.MaxRunVelocity - m_RigidBody2D.velocity.x) > m_ChillThresholdVelocity)
                {
                    if (m_RigidBody2D.velocity.x < platformData.MaxRunVelocity)
                        m_RigidBody2D.totalForce = new Vector2(platformData.ReadyRunForce, m_RigidBody2D.totalForce.y);
                    else
                        m_RigidBody2D.totalForce = new Vector2(-platformData.RunBrakeForce, m_RigidBody2D.totalForce.y);
                }
            }

            return;
        }

        if (state == KuraState.Fly)
        {
            if (Math.Abs(m_MaxFlyVelocity - m_RigidBody2D.velocity.x) > m_ChillThresholdVelocity)
                m_RigidBody2D.totalForce = new Vector2(m_FlyForce, m_RigidBody2D.totalForce.y);

            return;
        }

        if (state == KuraState.Glide)
        {
            if (Math.Abs(m_MaxFlyVelocity - m_RigidBody2D.velocity.x) > m_ChillThresholdVelocity)
                m_RigidBody2D.totalForce = new Vector2(-m_FlyBrakeForce, m_RigidBody2D.totalForce.y);

            return;
        }

        if (state == KuraState.Boosted)
        {
            if (m_CurSpeedBoostTime > 0)
            {
                m_CurSpeedBoostTime -= Time.fixedDeltaTime;
                m_RigidBody2D.totalForce = new Vector2(m_CurSpeedBoostForce, m_RigidBody2D.totalForce.y);
            }
            else
            {
                m_GeneralBase.State = KuraState.None;
                m_GeneralBase.State = GetNewKuraState();

                m_CurSpeedBoostTime = 0;
            }

            return;
        }

        Debug.Log("No kura state ??? " + m_GeneralBase.LocalData.state);

        return;
    }

    public void Respawn()
    {
        m_Kura.transform.position = m_GeneralBase.LocalData.spawnData.position;
        m_RigidBody2D.velocity = m_GeneralBase.LocalData.spawnData.velocity;
        m_GravityDirection = m_GeneralBase.LocalData.spawnData.gravityDirection;
        m_GravityMultiplier = m_GeneralBase.LocalData.spawnData.gravityMultiplier;
        Flip(m_GravityDirection);
    }

    public void Boost(SpeedBoostScriptableObject speedBoostData)
    {
        m_GeneralBase.State = KuraState.Boosted;
        m_CurSpeedBoostTime = speedBoostData.boostTime;
        m_CurSpeedBoostForce = speedBoostData.boostForce;
    }

    // Stop kura when it finishes
    public void Finish()
    {
        //if (!(IsClient && IsOwner))
        //    return;

        m_GravityMultiplier = 0.0f;
        m_RigidBody2D.gravityScale = m_GravityMultiplier;

        m_RigidBody2D.velocity = Vector2.zero;
        m_Kura.transform.position += new Vector3(UnityEngine.Random.Range(m_FinishTPDistance.x, m_FinishTPDistance.y), 0f, 0f);
    }

    public void ApplyTransformData(KuraTransfromData localTransformData)
    {
        m_Kura.transform.position = localTransformData.position;
        m_RigidBody2D.velocity = localTransformData.velocity;

        m_GravityDirection = localTransformData.gravityDirection;
        m_GravityMultiplier = localTransformData.gravityMultiplier;
        m_RigidBody2D.gravityScale = m_GravityDirection * m_GravityMultiplier;

        m_NFlips = localTransformData.nFlips;

        Flip(localTransformData.gravityDirection);
    }

    public KuraTransfromData GetTransformData()
    {
        KuraTransfromData tr = new ();
        tr.gravityDirection = m_GravityDirection;
        tr.gravityMultiplier = m_GravityMultiplier;
        tr.position = m_Kura.transform.position;
        tr.velocity = m_RigidBody2D.velocity;
        tr.nFlips = m_NFlips;

        return tr;
    }

    public void GiveFlip()
    {
        m_NFlips++;
        m_NFlips = Math.Min(m_NFlips, m_MaxFlips);
    }
}