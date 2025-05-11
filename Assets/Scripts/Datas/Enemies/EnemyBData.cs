using System;
using UnityEngine;

[Serializable]
public class EnemyBData : EnemyData
{
    [Header("Enemy B Prefab")]
    public GameObject Prefab;

    [Header("Bomb Settings")]
    public float bombCooldown;
    public float detectionMinAngle;
    public float detectionMaxAngle;
}
