using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnEmitters : MonoBehaviour
{
    public Transform SpawnerTransform;
    public int NumberOfEmitters = 115;
    public float DeltaX = 15;
    public GameObject SnowEmitter;

    void Start()
    {
        for(int i = 0; i < NumberOfEmitters; i++)
            Instantiate(SnowEmitter, new Vector3(SpawnerTransform.position.x + i * DeltaX, SpawnerTransform.position.y, 0), Quaternion.identity);
    }

}
