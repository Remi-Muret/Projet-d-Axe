using System;
using UnityEngine;

[Serializable]
public class EnemyAData : EnemyData
{
    [Header("Charge Settings")]
    public float chargeSpeed;
    public float chargeDuration;
    public float chargeCooldown;
    public float hitstunDuration;
}
