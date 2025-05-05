using System;
using UnityEngine;

[Serializable]
public class EnemyBData : EnemyData
{
    [Header("Bomb Settings")]
    public float bombCooldown;
    public float detectionMinAngle;
    public float detectionMaxAngle;
}
