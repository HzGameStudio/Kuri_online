using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;
using TMPro;
using Unity.Collections;
using UnityEngine.UI;
using System.Linq;

// This class is for storing the general information about the game, such as:
// The lobby code, references to all the players, is the game running bool, etc.
public class GameData : NetworkBehaviour
{
    // <NetworkVariable>s are generally variables that update often and other scripts use them often, they are automatically syncronized
    // among all the clients (by the way they can only be changed by the Server(Host)
    // and for a client to change them, you need a [SeverRPC] (see PlayerControl.cs (211)))
    public NetworkVariable<int> numPlayersInGame = new NetworkVariable<int>();
    public NetworkVariable<bool> isGameRunning = new NetworkVariable<bool>(false);
    public NetworkVariable<FixedString128Bytes> lobbyCode = new NetworkVariable<FixedString128Bytes>("IF YOU SEE THIS THEN YOU'RE OFFLINE, YARIK FORGOT TO CHANGE UNITY TRANSFORM PROTOCOL TYPE");

    // Non-<NetworkVariable> variables usually don't update on run-time,
    // so they are the same when the clients starts so it doesn't need to sync
    public int maxPlayers = 4;

    // These variables are used to give players that spawn in links to UI elements, so we don't have to run GameObject.Find() every time
    public GameObject startButtonGameObject;

    public TextMeshProUGUI playerIDText;

    public TextMeshProUGUI winnerText;

    public TextMeshProUGUI playerRunTimeText;

    public TextMeshProUGUI lobbyIDText;

    public TextMeshProUGUI kuraStatetext;

    public GameObject miniMapGameObject;

    public GameObject SpactatorModeButton;
    public GameObject SpactatorModeHolder;

    // Maybe have to do list of <PlayerData> ? Ask yarik later
    // This is kind of useless for now, but the idea is good lemao
    public List<GameObject> m_PlayerDataList = new List<GameObject>();

    // All positions that a player can spawn in
    [SerializeField]
    private List<Vector3> m_SpawnPosTransformList = new List<Vector3>();

    // Currently available positions to spawn, position is removed when player spawns there, and the list is reset to full when map spawns
    [SerializeField]
    private List<Vector3> m_CurGameSpawnPosTransformList;

    public int numFinishedPlayers = 0;

    private void Start()
    {
        // Get list of all spawn position on map
        GameObject[] SpawnPointList = GameObject.FindGameObjectsWithTag("spawnPoint");
        for (int i = 0; i < SpawnPointList.Length; i++) 
        {
            Debug.Log(SpawnPointList[i].transform.position);
            m_SpawnPosTransformList.Add(SpawnPointList[i].transform.position);
        }

        m_CurGameSpawnPosTransformList = new List<Vector3>(m_SpawnPosTransformList);

        Shuffle<Vector3>(m_CurGameSpawnPosTransformList);
    }

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

    // Just algorhythm to shuffle a list, copied from internet 
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


    public int FindSpactatorModeIndex(int playerIndex, int currentIndex, int shiftDirection)
    {
        int index = -1;
        if(currentIndex == -1)
        {
            int i = 0;
            foreach(GameObject player in m_PlayerDataList) 
            {
                
                if(!player.GetComponent<PlayerData>().finishedgame.Value)
                {
                    index = i;
                    return index;
                }
                i++;
            }
        }else
        {
            for(int i=1;i<m_PlayerDataList.Count;i++)
            {
                if(!m_PlayerDataList.ElementAt((currentIndex+i)%m_PlayerDataList.Count).GetComponent<PlayerData>().finishedgame.Value)
                {
                    return i + currentIndex;
                }
            }
        }

        



        return index;
    }



}
 