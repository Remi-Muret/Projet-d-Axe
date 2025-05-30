using System;
using UnityEngine;

[Serializable]
public class PlayerData : ObjectData
{
    [Header("Movement")]
    public float moveSpeed;
    public float jumpForce;
    public float jumpDuration;
    public float wallSlideGravity;
    public float wallJumpForceX;
    public float wallJumpForceY;

    [Header("Health")]
    public int health;
    public float respawnDelay;
    public float invulnerabilityTime;
    public float knockbackX;
    public float knockbackY;

    [Header("Bomb Attack")]
    public int bombDamage;
    public float bombForce;
    public float bombAngle;
    public float bombCooldown;

    [Header("Melee Attack")]
    public int meleeDamage;
    public float attackChargeRate;
    public float dashSpeed;
    public float minAttackDuration;
    public float maxAttackDuration;

    [Header("Projectile Attack")]
    public int projectileDamage;
    public float projectileForce;
    public float projectileCooldown;
}
