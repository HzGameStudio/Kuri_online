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

[Serializable]
public struct MainSceneObjectsCache
{
    public TextMeshProUGUI playerIDText;
    public TextMeshProUGUI winnerText;
    public TextMeshProUGUI playerRunTimeText;
    public TextMeshProUGUI lobbyIDText;
    public TextMeshProUGUI kuraStatetext;
    public GameObject miniMapGameObject;
    public GameObject SpectatorModeButton;
    public GameObject SpectatorModeHolder;
}

public class MainManager : SingletonNetwork<MainManager>
{
    // Hide in inspector because inspector bug with NetworkVariable
    [HideInInspector]
    public NetworkVariable<int> numPlayersInGame = new NetworkVariable<int>();

    public MainSceneObjectsCache sceneObjectsCache;

    public List<ulong> playerIds = new List<ulong>();

    public List<PlayerMain> PlayerMainList = new List<PlayerMain>();

    // All positions that a player can spawn in
    private List<Vector3> m_SpawnPosTransformList = new List<Vector3>();

    // Currently available positions to spawn, position is removed when player spawns there
    private List<Vector3> m_CurGameSpawnPosTransformList;

    public int numFinishedPlayers = 0;

    [SerializeField]
    private GameObject m_PlayerPrefab;

    public Vector3 GetSpawnPosition()
    {
        Vector3 pos = m_CurGameSpawnPosTransformList[0];
        m_CurGameSpawnPosTransformList.RemoveAt(0);
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
        for (int i = 1; i <= PlayerMainList.Count; i++) 
        {
            if (!PlayerMainList[(currentIndex + i) % PlayerMainList.Count].serverData.Value.finishedGame)
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

        Debug.Log(SpawnPointList.Length);

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

            Debug.Log("Game object " + go);

            PlayerMain pm = go.GetComponent<PlayerMain>();

            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { playerIds[i] }
                }
            };

            pm.SendKuraDataToClientRPC(i + 1, spawnPos, clientRpcParams);
        }
    }
}
 