using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NewBehaviourScript : NetworkBehaviour
{
    [SerializeField]
    private GameObject objectPrefab;

    [SerializeField]
    private int maxObjectInstanceCount = 3;

    private void Awake()
    {
        //NetworkManager.Singleton.OnServerStarted += () =>
        //{
        //    NetworkObjectPool.Instance.InitializePool();
        //};
    }

    public void SpawnObjects()
    {
        if (!IsServer) return;

        for (int i = 0; i < maxObjectInstanceCount; i++)
        {
            GameObject go = Instantiate(objectPrefab,
                new Vector3(Random.Range(-10, 10), 10.0f, Random.Range(-10, 10)), Quaternion.identity);
            //GameObject go = NetworkObjectPool.Instance.GetNetworkObject(objectPrefab).gameObject;
            //go.transform.position = new Vector3(Random.Range(-10, 10), 10.0f, Random.Range(-10, 10));
            go.GetComponent<NetworkObject>().Spawn();
        }
    }

}
