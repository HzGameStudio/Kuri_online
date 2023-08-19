using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "PlatformScreaptebleObject", menuName = "ScriptableObjects/PlatformScreaptebleObject", order = 1)]
public class PlatformScreaptebleObject : ScriptableObject
{
    public float m_MaxFlapRunTime;

    public string platformType;

    public float m_MaxRunVelocity;

    public float m_RunForce;

    public float m_ReadyRunForce;

    public float m_RunBrakeForce;
}
