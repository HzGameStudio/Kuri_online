using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;
using TMPro;
using Unity.Collections;
using UnityEngine.UI;
using System.Linq;
using System;

public class GameManager : SingletonNetworkPersistent<GameManager>
{
    [HideInInspector]
    public NetworkVariable<FixedString128Bytes> lobbyCode = new NetworkVariable<FixedString128Bytes>("IF YOU SEE THIS THEN YOU'RE OFFLINE, YARIK FORGOT TO CHANGE UNITY TRANSFORM PROTOCOL TYPE");

    public int maxPlayers = 4;

    public NetworkVariable<int> connectedPlayers;

    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += UpdateConnectedPlayers;
        NetworkManager.Singleton.OnClientDisconnectCallback += UpdateConnectedPlayers;
    }

    private void UpdateConnectedPlayers(ulong clientID)
    {
        if (!IsServer) return;
        connectedPlayers.Value = NetworkManager.Singleton.ConnectedClients.Count;
    }
}
