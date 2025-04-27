using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Scriptable Objects/PlayerData", order = 1)]
public class PlayerData : ScriptableObject
{
    [Header("Set up")]
	public float scaleCoef;
    public float gravity;
	public Sprite sprite;
    public Color color;

    [Header("Movement")]
    public float wallSlideGravity;
    public float moveSpeed;
    public float jumpForce;
    public float jumpDuration;
    public float wallJumpForceX;
    public float wallJumpForceY;

    [Header("Health")]
    public int health;
    public float respawnDelay;
    public float invulnerabilityTime;
    public float knockbackX;
    public float knockbackY;

    [Header("Melee Attack")]
    public int meleeDamage;
    public float attackChargeRate;
    public float dashSpeed;
    public float minAttackDuration;
    public float maxAttackDuration;

    [Header("Range Attack")]
    public int projectileDamage;
    public float projectileForce;
    public float projectileCooldown;

    [Header("Bomb Attack")]
    public int bombDamage;
    public float bombForce;
    public float bombAngle;
    public float bombCooldown;
}
