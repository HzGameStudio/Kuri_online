using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
// Network variables should be value objects


public class ClientPredictionController : NetworkBehaviour
{
    [SerializeField]
    private int m_MaxFlips = 1;

    private int m_GravityDirection = 1;
 

    [SerializeField]
    private string[] m_FlipTagList = { "simplePlatform", "player" };

    private int m_NFlips = 1;
    public struct InputPayload : INetworkSerializable
    {
        public int tick;
        public DateTime timestamp;
        public ulong networkObjectId;
        public bool inputVector;
        public Vector3 position;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref tick);
            serializer.SerializeValue(ref timestamp);
            serializer.SerializeValue(ref networkObjectId);
            serializer.SerializeValue(ref inputVector);
            serializer.SerializeValue(ref position);
        }
    }

    public struct StatePayload : INetworkSerializable
    {
        public int tick;
        public ulong networkObjectId;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 velocity;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref tick);
            serializer.SerializeValue(ref networkObjectId);
            serializer.SerializeValue(ref position);
            serializer.SerializeValue(ref rotation);
            serializer.SerializeValue(ref velocity);
        }
    }

    [SerializeField] GameObject Camera;
    Rigidbody2D rb;
    float reconciliationThreshold = 1f;

    const float thresholdSpeed = 10f;

    // Netcode general
    NetworkTimer networkTimer;
    const float k_serverTickRate = 60f; // 60 FPS
    const int k_bufferSize = 1024;

    // Netcode client specific
    CircularBuffer<StatePayload> clientStateBuffer;
    CircularBuffer<InputPayload> clientInputBuffer;
    StatePayload lastServerState;
    StatePayload lastProcessedState;

    //ClientNetworkTransform clientNetworkTransform;

    // Netcode server specific
    CircularBuffer<StatePayload> serverStateBuffer;
    Queue<InputPayload> serverInputQueue;

    [Header("Netcode")]
    [SerializeField] GameObject serverCube;
    [SerializeField] GameObject clientCube;

    void Awake()
    {

        rb = GetComponent<Rigidbody2D>();
        //clientNetworkTransform = GetComponent<ClientNetworkTransform>();




        networkTimer = new NetworkTimer(k_serverTickRate);
        clientStateBuffer = new CircularBuffer<StatePayload>(k_bufferSize);
        clientInputBuffer = new CircularBuffer<InputPayload>(k_bufferSize);

        serverStateBuffer = new CircularBuffer<StatePayload>(k_bufferSize);
        serverInputQueue = new Queue<InputPayload>();


    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            Camera.SetActive(false);
            return;
        }

        //turn on camera
    }

    void Update()
    {
        networkTimer.Update(Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.Q))
        {
            transform.position += transform.forward * 20f;
        }
    }

    void FixedUpdate()
    {
        while (networkTimer.ShouldTick())
        {
            HandleClientTick();
            HandleServerTick();
        }

    }

    void HandleServerTick()
    {
        if (!IsServer) return;

        var bufferIndex = -1;
        InputPayload inputPayload = default;
        while (serverInputQueue.Count > 0)
        {
            inputPayload = serverInputQueue.Dequeue();

            bufferIndex = inputPayload.tick % k_bufferSize;

            StatePayload statePayload = ProcessMovement(inputPayload);
            serverStateBuffer.Add(statePayload, bufferIndex);
        }

        if (bufferIndex == -1) return;
        SendToClientRpc(serverStateBuffer.Get(bufferIndex));
        //HandleExtrapolation(serverStateBuffer.Get(bufferIndex), CalculateLatencyInMillis(inputPayload));
    }

    static float CalculateLatencyInMillis(InputPayload inputPayload) => (DateTime.Now - inputPayload.timestamp).Milliseconds / 1000f;


    [ClientRpc]
    void SendToClientRpc(StatePayload statePayload)
    {
        //clientRpcText.SetText($"Received state from server Tick {statePayload.tick} Server POS: {statePayload.position}"); 
        //serverCube.transform.position = statePayload.position.With(y: 4);
        if (!IsOwner) return;
        lastServerState = statePayload;
    }

    void HandleClientTick()
    {
        if (!IsClient || !IsOwner) return;

        var currentTick = networkTimer.CurrentTick;
        var bufferIndex = currentTick % k_bufferSize;

        InputPayload inputPayload = new InputPayload()
        {
            tick = currentTick,
            timestamp = DateTime.Now,
            networkObjectId = NetworkObjectId,
            inputVector = GetInput(),
            position = transform.position
        };

        clientInputBuffer.Add(inputPayload, bufferIndex);
        SendToServerRpc(inputPayload);

        StatePayload statePayload = ProcessMovement(inputPayload);
        clientStateBuffer.Add(statePayload, bufferIndex);

        HandleServerReconciliation();
    }

    bool ShouldReconcile()
    {
        bool isNewServerState = !lastServerState.Equals(default);
        bool isLastStateUndefinedOrDifferent = lastProcessedState.Equals(default)
                                               || !lastProcessedState.Equals(lastServerState);

        return isNewServerState && isLastStateUndefinedOrDifferent;
    }

    void HandleServerReconciliation()
    {
        if (!ShouldReconcile()) return;

        float positionError;
        int bufferIndex;

        bufferIndex = lastServerState.tick % k_bufferSize;
        if (bufferIndex - 1 < 0) return; // Not enough information to reconcile

        StatePayload rewindState = IsHost ? serverStateBuffer.Get(bufferIndex - 1) : lastServerState; // Host RPCs execute immediately, so we can use the last server state
        StatePayload clientState = IsHost ? clientStateBuffer.Get(bufferIndex - 1) : clientStateBuffer.Get(bufferIndex);
        positionError = Vector3.Distance(rewindState.position, clientState.position);

        if (positionError > reconciliationThreshold)
        {
            ReconcileState(rewindState);
            //reconciliationTimer.Start();
        }

        lastProcessedState = rewindState;
    }

    void ReconcileState(StatePayload rewindState)
    {
        transform.position = rewindState.position;
        transform.rotation = rewindState.rotation;
        rb.velocity = rewindState.velocity;

        if (!rewindState.Equals(lastServerState)) return;

        clientStateBuffer.Add(rewindState, rewindState.tick % k_bufferSize);

        // Replay all inputs from the rewind state to the current state
        int tickToReplay = lastServerState.tick;

        while (tickToReplay < networkTimer.CurrentTick)
        {
            int bufferIndex = tickToReplay % k_bufferSize;
            StatePayload statePayload = ProcessMovement(clientInputBuffer.Get(bufferIndex));
            clientStateBuffer.Add(statePayload, bufferIndex);
            tickToReplay++;
        }
    }

    [ServerRpc]
    void SendToServerRpc(InputPayload input)
    {
        //serverRpcText.SetText($"Received input from client Tick: {input.tick} Client POS: {input.position}");
        //clientCube.transform.position = input.position.With(y: 4);
        serverInputQueue.Enqueue(input);
    }

    StatePayload ProcessMovement(InputPayload input)
    {
        Move(input.inputVector);

        return new StatePayload()
        {
            tick = input.tick,
            networkObjectId = NetworkObjectId,
            position = transform.position,
            rotation = transform.rotation,
            velocity = rb.velocity,
        };
    }

    void Move(bool input)
    {
        if (input)
        {
            // Flip
            if (m_NFlips > 0)
            {
                m_GravityDirection *= -1;
                rb.gravityScale = m_GravityDirection;
                //m_Transform.localScale = new Vector3(m_Transform.localScale.x, m_Transform.localScale.y * -1, m_Transform.localScale.z);

                m_NFlips--;
            }
        }
    }

    bool GetInput()
    {
        if (Input.GetKeyDown(KeyCode.W)) return true;
        return false;
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsServer && !IsOwner) return;
        // Check if the platform hit is eligible to give a flip
        if (Array.Exists(m_FlipTagList, element => collision.gameObject.CompareTag(element)))
        {
            m_NFlips++;
            m_NFlips = Math.Min(m_NFlips, m_MaxFlips);
        }



    }
}
