using System;
using UnityEngine;

[Serializable]
public class EnemyAData
{
	public string label;

	[Header("Set up")]
	public float scaleCoef;
    public float gravity;
	public Sprite sprite;
	public Color color;

	[Header("Stats")]
    public float moveSpeed;
    public float jumpForce;
    public float hitstunDuration;
	public int health;
    public int meleeDamage;

    [Header("Patrol Settings")]
    public float waitDuration;
    public Vector2 patrolPointA;
    public Vector2 patrolPointB;

    [Header("Charge Settings")]
    public float detectionRadius;
    public float chargeSpeed;
    public float chargeDuration;
    public float chargeCooldown;
}
