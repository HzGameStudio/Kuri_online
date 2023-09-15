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

    private IEnumerator Start()
    {
        //shouldnt be here?

        // Wait for the network Scene Manager to start
        yield return new WaitUntil(() => NetworkManager.Singleton.SceneManager != null);

        // Set the events on the loading manager
        // Doing this because every time the network session ends the loading manager stops
        // detecting the events
        LoadingSceneManager.Instance.Init();
    }
}
