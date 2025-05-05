using System.Collections;
using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    public enum CHARACTER
    {
        Player,
        EnemyA,
        EnemyB,
    }

    [SerializeField] private CHARACTER characterType;
    [SerializeField] private GameObject vfxPrefab;

    private int _health;
    private float _respawnDelay;
    private Vector2 _spawnPosition;
    private SpriteRenderer _spriteRenderer;
    private Color _originalColor;

    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer != null)
            _originalColor = _spriteRenderer.color;

        switch (characterType)
        {
            case CHARACTER.Player:
                _health = GameManager.Instance.GetPlayerData().health;
                _respawnDelay = GameManager.Instance.GetPlayerData().respawnDelay;
                break;
            case CHARACTER.EnemyA:
                int enemyAId = GetComponent<EnemyAController>().Id;
                _health = GameManager.Instance.GetEnemyAData(enemyAId).health;
                break;
            case CHARACTER.EnemyB:
                int enemyBId = GetComponent<EnemyBController>().Id;
                _health = GameManager.Instance.GetEnemyBData(enemyBId).health;
                break;
        }
    }

    void Die()
    {
        if (vfxPrefab != null)
        {
            float vfxLifetime = 0.1f;
            GameObject vfxInstance = Instantiate(vfxPrefab, transform.position, Quaternion.identity);
            Vector3 scale = vfxInstance.transform.localScale;
            if (Random.value < 0.5f) scale.x *= -1;
            if (Random.value < 0.5f) scale.y *= -1;
            vfxInstance.transform.localScale = scale;
            Destroy(vfxInstance, vfxLifetime);
        }

        switch (characterType)
        {
            case CHARACTER.Player:
                Vector2 respawnPosition = GameManager.Instance.GetLastCheckpointPosition();
                GameManager.Instance.StartCoroutine(GameManager.Instance.RespawnPlayerAfterDelay(_respawnDelay, respawnPosition));
                Destroy(gameObject);
                break;

            default:
                Destroy(gameObject);
                break;
        }
    }

    IEnumerator FlashRed()
    {
        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            _spriteRenderer.color = _originalColor;
        }
    }
    
    public void TakeDamage(int damage)
    {
        _health -= damage;
        StartCoroutine(FlashRed());

        if (_health <= 0) Die();
    }

    public void HealDamage(int amount)
    {
        int maxHealth = GameManager.Instance.GetPlayerData().health;
        _health = Mathf.Min(_health + amount, maxHealth);
    }
}
