using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using System;
using System.Security.Cryptography;
using UnityEngine.UI;

[Serializable]
public struct MainSceneObjectsCache
{
    public TextMeshProUGUI playerIDText;
    public TextMeshProUGUI winnerText;
    public TextMeshProUGUI playerRunTimeText;
    public TextMeshProUGUI perfectRunTimeText;
    public TextMeshProUGUI lobbyIDText;
    public TextMeshProUGUI kuraSpeedtext;
    public GameObject miniMapGameObject;
    public GameObject SpectatorModeButton;
    public GameObject SpectatorModeHolder;
    public GameObject restartButton;
}

public class MainManager : SingletonNetwork<MainManager>
{
    // Hide in inspector because inspector bug with NetworkVariable
    [HideInInspector]
    public NetworkVariable<int> numPlayersInGame = new ();

    public MainSceneObjectsCache sceneObjectsCache;

    public List<ulong> playerIds = new ();

    public List<O_PlayerMain> PlayerMainList = new ();

    // All positions that a player can spawn in
    private List<Vector3> m_SpawnPosTransformList = new ();

    // Currently available positions to spawn, position is removed when player spawns there
    private List<Vector3> m_CurGameSpawnPosTransformList;

    public int numFinishedPlayers = 0;

    [SerializeField]
    private GameObject m_PlayerPrefab;

    public new void Awake()
    {
        base.Awake();

        sceneObjectsCache.restartButton.GetComponent<Button>().onClick.AddListener(RestartGame);
    }

    public void RestartGame()
    {
        LoadingSceneManager.Instance.LoadScene(SceneName.O_GameMenu, true);
    }

    public Vector3 GetSpawnPosition()
    {
        Vector3 pos = m_CurGameSpawnPosTransformList[0];
        m_CurGameSpawnPosTransformList.RemoveAt(0);
        if (m_CurGameSpawnPosTransformList.Count == 0)
        {
            m_CurGameSpawnPosTransformList = new List<Vector3>(m_SpawnPosTransformList);
        }
        return pos;
    }

    public static void Shuffle<T>(in IList<T> list)
    {
        System.Random rng = new System.Random();

        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public int FindSpactatorModeIndex(int currentIndex)
    {
        for (int i=0; i<PlayerMainList.Count; i++)
            Debug.Log("List " + PlayerMainList[i]);

        for (int i = 1; i <= PlayerMainList.Count; i++) 
        {
            if (!PlayerMainList[(currentIndex + i) % PlayerMainList.Count].ServerData.finishedGame)
                return (currentIndex + i) % PlayerMainList.Count;
        }
        return currentIndex;
    }

    public void MainSceneInitialize(ulong clientId)
    {
        numPlayersInGame.Value++;

        playerIds.Add(clientId);

        Debug.Log("Adding client " + clientId);

        // Check if is the last client
        if (numPlayersInGame.Value != NetworkManager.Singleton.ConnectedClients.Count)
            return;

        // Get list of all spawn position on map
        GameObject[] SpawnPointList = GameObject.FindGameObjectsWithTag("spawnPoint");

        for (int i = 0; i < SpawnPointList.Length; i++)
        {
            m_SpawnPosTransformList.Add(SpawnPointList[i].transform.position);
        }

        m_CurGameSpawnPosTransformList = new List<Vector3>(m_SpawnPosTransformList);

        Shuffle<Vector3>(m_CurGameSpawnPosTransformList);

        Debug.Log("Players in game " + numPlayersInGame.Value);

        for (int i = 0; i < numPlayersInGame.Value; i++)
        {
            Vector3 spawnPos = GetSpawnPosition();

            Debug.Log("Spawn pos " + spawnPos);

            Debug.Log("Player ID " + playerIds[i]);

            GameObject go = NetworkObjectSpawner.SpawnNewNetworkObjectAsPlayerObject(
                m_PlayerPrefab,
                spawnPos,
                playerIds[i],
                true);

            O_PlayerMain pm = go.GetComponent<O_PlayerMain>();

            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { playerIds[i] }
                }
            };

            pm.SetInitialDataToClientRPC((int)playerIds[i], spawnPos, clientRpcParams);
        }
    }
}
 