using System;
using UnityEngine;

[Serializable]
public class EnemyData : ObjectData
{
	[Header("Stats")]
    public float moveSpeed;
    public float jumpForce;
	public int health;
    public int meleeDamage;

    [Header("Patrol Settings")]
    public float waitDuration;
    public float detectionRadius;
    public Vector2 patrolPointA;
    public Vector2 patrolPointB;
}
