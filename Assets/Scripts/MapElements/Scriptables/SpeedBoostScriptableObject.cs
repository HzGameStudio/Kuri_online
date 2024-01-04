using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpeedBoostScriptableObject", menuName = "ScriptableObjects/SpeedBoostScriptableObject", order = 2)]
public class SpeedBoostScriptableObject : ScriptableObject
{
    public float boostTime;

    public float boostForce;
}
