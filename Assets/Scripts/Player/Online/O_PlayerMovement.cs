using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class O_PlayerMovement : PlayerMovementBase
{
    [DoNotSerialize, HideInInspector]
    private NetworkVariable<KuraTransfromData> m_TransformFromClient;

    public Vector2 Velocity { get { return m_RigidBody2D.velocity; } }

    public O_PlayerMovement(GameObject kura, Transform skinTransform) : base(kura, skinTransform) { }

    public void OnNetworkSpawn(NetworkVariable<KuraTransfromData> transformFromClient)
    {
        m_TransformFromClient = transformFromClient;
    }

    public void UpdatePositionOnServerRPC(KuraTransfromData localTransformData, bool IsOwner)
    {
        m_TransformFromClient.Value = localTransformData;

        if (IsOwner)
            return;

        // updates data on server for all kuras except host

        ApplyTransformData(localTransformData);
    }
}
