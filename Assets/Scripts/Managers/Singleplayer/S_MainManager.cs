using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[Serializable]
public struct S_MainSceneObjectsCache
{
    public TextMeshProUGUI winnerText;
    public TextMeshProUGUI playerRunTimeText;
    public TextMeshProUGUI perfectRunTimeText;
    public TextMeshProUGUI kuraSpeedtext;
    public GameObject miniMapGameObject;
    public GameObject restartButton;
}

//TODO: Create namespaces for singleplayer and multiplayer

public class S_MainManager : Singleton<S_MainManager>
{
    [SerializeField]
    private GameObject m_PlayerPrefab;

    public S_MainSceneObjectsCache sceneObjectsCache;

    // All positions that a player can spawn in
    private List<Vector3> m_SpawnPosTransformList = new();

    // Currently available positions to spawn, position is removed when player spawns there
    private List<Vector3> m_CurGameSpawnPosTransformList;

    private new void Awake()
    {
        base.Awake();

        MapManager.Instance.LoadMap(MapManager.Instance.SelectedMapName).Completed += (a) =>
        {
            // Get list of all spawn position on map
            GameObject[] SpawnPointList = GameObject.FindGameObjectsWithTag("spawnPoint");

            for (int i = 0; i < SpawnPointList.Length; i++)
            {
                m_SpawnPosTransformList.Add(SpawnPointList[i].transform.position);
            }

            m_CurGameSpawnPosTransformList = new List<Vector3>(m_SpawnPosTransformList);

            Shuffle<Vector3>(m_CurGameSpawnPosTransformList);

            GameObject player = Instantiate(m_PlayerPrefab, GetSpawnPosition(), Quaternion.identity);

            gameObject.GetComponent<PauseMenu>().player = player;
        };
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
}
