using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpawnPlayers : MonoBehaviour
{
    public GameObject m_playerPrefab;

    public float m_minX;
    public float m_maxX;

    private void Start()
    {
        Vector2 randomPosition = new Vector2(Random.Range(m_minX, m_maxX), 0);
        PhotonNetwork.Instantiate(m_playerPrefab.name, randomPosition, Quaternion.identity);
    }
}
