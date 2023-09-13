using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;
using TMPro;
using Unity.Collections;
using UnityEngine.UI;
using System.Linq;

// NOTE: SINGLETON THIS

// This class is for storing the general information about the game, such as:
// The lobby code, references to all the players, is the game running bool, etc.
public class GameData : NetworkBehaviour
{
    // Hide in inspector because inspector bug with NetworkVariable
    [HideInInspector]
    public NetworkVariable<int> numPlayersInGame = new NetworkVariable<int>();
    [HideInInspector]
    public NetworkVariable<bool> isGameRunning = new NetworkVariable<bool>(false);
    [HideInInspector]
    public NetworkVariable<FixedString128Bytes> lobbyCode = new NetworkVariable<FixedString128Bytes>("IF YOU SEE THIS THEN YOU'RE OFFLINE, YARIK FORGOT TO CHANGE UNITY TRANSFORM PROTOCOL TYPE");

    public int maxPlayers = 4;

    // NOTE: SINGLETON THIS

    [System.Serializable]
    public struct SceneObjectsCache
    {
        public GameObject startButtonGameObject;
        public TextMeshProUGUI playerIDText;
        public TextMeshProUGUI winnerText;
        public TextMeshProUGUI playerRunTimeText;
        public TextMeshProUGUI lobbyIDText;
        public TextMeshProUGUI kuraStatetext;
        public GameObject miniMapGameObject;
        public GameObject SpactatorModeButton;
        public GameObject SpactatorModeHolder;
    }

    public SceneObjectsCache sceneObjectsCache;

    public List<PlayerData> playerDataList = new List<PlayerData>();

    // All positions that a player can spawn in
    private List<Vector3> m_SpawnPosTransformList = new List<Vector3>();

    // Currently available positions to spawn, position is removed when player spawns there
    private List<Vector3> m_CurGameSpawnPosTransformList;

    public int numFinishedPlayers = 0;

    private void Start()
    {
        // Get list of all spawn position on map
        GameObject[] SpawnPointList = GameObject.FindGameObjectsWithTag("spawnPoint");
        for (int i = 0; i < SpawnPointList.Length; i++) 
        {
            m_SpawnPosTransformList.Add(SpawnPointList[i].transform.position);
        }

        m_CurGameSpawnPosTransformList = new List<Vector3>(m_SpawnPosTransformList);

        Shuffle<Vector3>(m_CurGameSpawnPosTransformList);
    }

    // NOTE: DISCOMBOBULATE

    public void CalcNumPlayersInGame()
    {
        GameObject[] playerList = GameObject.FindGameObjectsWithTag("player");
        numPlayersInGame.Value = playerList.Length;
    }

    public Vector3 GetSpawnPosition()
    {
        Vector3 pos = m_CurGameSpawnPosTransformList[0];
        m_CurGameSpawnPosTransformList.RemoveAt(0);
        return pos;
    }

    // This function is bound to the <StartButton>
    public void StartGame()
    {
        isGameRunning.Value = true;
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
        if(currentIndex == -1)
        {
            for (int i = 1; i <= playerDataList.Count; i++) 
            {
                if (!playerDataList[(currentIndex + i) % playerDataList.Count].finishedGame.Value)
                    return currentIndex + i;
            }
        }
        return currentIndex;
    }
}
 