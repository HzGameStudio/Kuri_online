using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "PlatformScreaptebleObject", menuName = "ScriptableObjects/PlatformScreaptebleObject", order = 1)]
public class PlatformScreaptebleObject : ScriptableObject
{
    public string platformType;

    public float MaxRunVelocity;

    public float RunForce;

    public float ReadyRunForce;

    public float RunBrakeForce;

    public float damage;

    public float deltaTimeDamage; //time betwen to damages
}
