using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayerMain
{
    public void Finish();

    public void Damage(float damage);
    public bool SetCheckPoint(KuraTransfromData spawnData);
    public bool SetCheckPoint();
    public void Boost(SpeedBoostScriptableObject speedBoostData);
}