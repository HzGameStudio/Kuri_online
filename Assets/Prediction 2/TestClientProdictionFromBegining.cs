using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Windows;

public class TestClientProdictionFromBegining : NetworkBehaviour
{
    Rigidbody2D rb;

    bool requestMovement = false;

    [SerializeField] GameObject MainCamera;

    private int tick = 0;
    private float tickRate = 1f / 60f;
    private float tickDeltaTime = 0f;

    private const int buffer = 1024;

    private HandleState.InputState[] inputStates = new HandleState.InputState[buffer];

    private HandleState.TransformStateRW[] transformStates = new HandleState.TransformStateRW[buffer];

    public NetworkVariable<HandleState.TransformStateRW> currentServerTransformState = new NetworkVariable<HandleState.TransformStateRW>();

    public HandleState.TransformStateRW previousTransformState;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            MainCamera.SetActive(true);
            return;
        }

        if(IsHost)
        {
            HandleState.TransformStateRW transformState = new()
            {
                tick = tick,
                finalPos = transform.position,
                isMoving = true
            };

            currentServerTransformState.Value = transformState;
        }
        
    }

    private void OnEnable()
    {
        currentServerTransformState.OnValueChanged += OnServerStateChanged;
    }

    private void ProcessLocalPlayerMovement(bool moveInput_input)
    {
        tickDeltaTime += Time.deltaTime;

        if (tickDeltaTime > tickRate)
        {
            int bufferIndex = tick % buffer;

            MovePlayerWithServerTickServerRPC(tick, moveInput_input);
            Move(moveInput_input);

            HandleState.InputState inputState = new()
            {
                tick = tick,
                moveInput = moveInput_input
            };

            HandleState.TransformStateRW transformState = new()
            {
                tick = tick,
                finalPos = transform.position,
                isMoving = true
            };

            inputStates[bufferIndex] = inputState;
            transformStates[bufferIndex] = transformState;

            tickDeltaTime -= tickRate;

            if (tick >= buffer)
                tick = 0;
            else
                tick++;
        }
    }

    [ServerRpc]
    private void MovePlayerWithServerTickServerRPC(int tick, bool moveInput_input)
    {
        //host check 
        if (!IsOwner)
            Move(moveInput_input);

        HandleState.TransformStateRW transformState = new()
        {
            tick = tick,
            finalPos = transform.position,
            isMoving = moveInput_input,
            gravityDirection = rb.gravityScale,
        };

        //Debug.Log(transformState.finalPos);

        previousTransformState = currentServerTransformState.Value;
        currentServerTransformState.Value = transformState;
    }

    public void SimulateOtherPlayers()
    {
        if (IsServer) return;
        tickDeltaTime += Time.deltaTime;

        if (tickDeltaTime > tickRate)
        {
            //check for reconsil


            //if dont need to reconcil
            if (currentServerTransformState.Value.isMoving || true)
                Debug.Log(currentServerTransformState.Value.gravityDirection);
                rb.gravityScale = currentServerTransformState.Value.gravityDirection;
                //transform.position = currentServerTransformState.Value.finalPos;

            tickDeltaTime -= tickRate;

            if (tick == buffer)
                tick = 0;
            else
                tick++;
        }
    }

    private void Update()
    {
        if (IsOwner)
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.W))
            {
                requestMovement = true;
            }
        }

        if (IsClient && IsLocalPlayer)
        {
            //Debug.Log(OwnerClientId);
            ProcessLocalPlayerMovement(requestMovement);
        }
        else
        {
            SimulateOtherPlayers();
        }
    }

    void Move(bool flip)
    {
        if (flip)
        {
            rb.gravityScale = rb.gravityScale * (-1);
            requestMovement = false;
        }
    }

    private void OnServerStateChanged(HandleState.TransformStateRW previousValue, HandleState.TransformStateRW newValue)
    {
        //Debug.Log("onvaluechanged" + OwnerClientId.ToString());
        //if (OwnerClientId == 2)
        // Debug.Log(previousValue.finalPos.ToString() + ' ' +  newValue.finalPos.ToString());
        previousTransformState = previousValue;
    }
}