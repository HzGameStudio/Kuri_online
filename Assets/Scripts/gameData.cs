using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;

public class gameData : NetworkBehaviour
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
    public GameObject startButton;


    public NetworkVariable<int> numPlayersInGame = new NetworkVariable<int>();

    public bool isGameRuning = false;

    [SerializeField]
    private List<Vector3> spawnPosTransformList = new List<Vector3>();
    private List<bool> spawnPosIsTakenList = new List<bool>();

    private void Start()
    { 

        //Get list of all spawn position on map
        GameObject[] SpawnPointList = GameObject.FindGameObjectsWithTag("spawnPoint");
        for(int i=0;i< SpawnPointList.Length;i++)
        {
            Debug.Log(SpawnPointList[i].transform.position);
            spawnPosTransformList.Add(SpawnPointList[i].transform.position);
            spawnPosIsTakenList.Add(false);
        }
    }

    public int numFinishedPlayers = 0;
    public void CalcNumPlayersInGame()
    {
        GameObject[] playerList = GameObject.FindGameObjectsWithTag("player");
        numPlayersInGame.Value = playerList.Length;
    }

    public Vector3 GetSpawnPosition()
    {
        System.Random random = new System.Random();
        int index = random.Next(spawnPosTransformList.Count);

        while(spawnPosIsTakenList[index])
        {
            index = random.Next(spawnPosTransformList.Count);
        }
        spawnPosIsTakenList[index] = true;


        return spawnPosTransformList[index];
    }

    public void StartGame()
    {
        isGameRuning = true;
    }
}
 