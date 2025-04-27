using System;
using UnityEngine;

[Serializable]
public class EnemyBData
{
	public string label;

	[Header("Set up")]
	public float scaleCoef;
    public float gravity;
	public Sprite sprite;
	public Color color;

	[Header("Stats")]
    public float moveSpeed;
	public int health;
    public int meleeDamage;

    [Header("Patrol Settings")]
    public float waitDuration;
    public Vector2 patrolPointA;
    public Vector2 patrolPointB;

    [Header("Bomb Settings")]
    public float detectionRadius;
    public float bombCooldown;
    public float detectionMinAngle;
    public float detectionMaxAngle;
}
