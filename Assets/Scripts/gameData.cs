using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;
using TMPro;
using Unity.Collections;

public class GameData : NetworkBehaviour
{
    //public struct SpawnPosition
    //{
    //    public SpawnPosition(Vector3 Position, bool IsTaken)
    //    {
    //        position = Position;
    //        isTaken = IsTaken;
    //    }

    //    public Vector3 position;
    //    public bool isTaken;
    //}

    public struct PlayerData
    {
        public PlayerData(GameObject GameObject, float RunTime, int PlaceInRuningGame)
        {
            runTime = RunTime;
            gameObject = GameObject;
            placeInRuningGame = new NetworkVariable<int>(PlaceInRuningGame);

        }
        public float runTime;
        public GameObject gameObject;
        public NetworkVariable<int> placeInRuningGame;
    }

    public int m_MaxPlayers = 4;

    public NetworkVariable<int> numPlayersInGame = new NetworkVariable<int>();
    public NetworkVariable<bool> isGameRunning = new NetworkVariable<bool>(false);
    public NetworkVariable<FixedString128Bytes> m_LobbyCode = new NetworkVariable<FixedString128Bytes>("IF YOU SEE THIS THEN YOU'RE OFFLINE, YARIK FORGOT TO CHANGE UNITY TRANSFORM PROTOCOL TYPE");

    public GameObject startButton;

    public TextMeshProUGUI playerIDText;

    public TextMeshProUGUI winnerText;

    public TextMeshProUGUI playerRunTimeText;

    public TextMeshProUGUI lobbyIDText;

    public TextMeshProUGUI kuraStatetext;

    public GameObject MiniMapGameObject;

    [SerializeField]
    private List<Vector3> spawnPosTransformList = new List<Vector3>();
    public List<PlayerData> playerDataList = new List<PlayerData>();

    [SerializeField]
    private List<Vector3> curGameSpawnPosTransformList;

    public int numFinishedPlayers = 0;

    private void Start()
    {
        //Get list of all spawn position on map
        GameObject[] SpawnPointList = GameObject.FindGameObjectsWithTag("spawnPoint");
        for (int i = 0; i < SpawnPointList.Length; i++) 
        {
            Debug.Log(SpawnPointList[i].transform.position);
            spawnPosTransformList.Add(SpawnPointList[i].transform.position);
        }

        curGameSpawnPosTransformList = new List<Vector3>(spawnPosTransformList);

        Shuffle<Vector3>(curGameSpawnPosTransformList);
    }

    public void CalcNumPlayersInGame()
    {
        GameObject[] playerList = GameObject.FindGameObjectsWithTag("player");
        numPlayersInGame.Value = playerList.Length;
    }

    public Vector3 GetSpawnPosition()
    {
        Vector3 pos = curGameSpawnPosTransformList[0];
        curGameSpawnPosTransformList.RemoveAt(0);
        return pos;
    }

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
}
 