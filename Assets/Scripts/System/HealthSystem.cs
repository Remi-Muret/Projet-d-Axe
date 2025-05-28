using System.Collections;
using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    public static HealthSystem Instance { get; private set; }

    public enum CHARACTER
    {
        Player,
        EnemyA,
        EnemyB,
    }

    [SerializeField] private CHARACTER characterType;
    [SerializeField] private GameObject killVFXPrefab;

    public HealthUI healthUI;
    public int _currentHealth;
    public int _maxHealth;

    private float _respawnDelay;
    private Vector2 _spawnPosition;
    private SpriteRenderer _spriteRenderer;
    private Color _originalColor;

    public static void SetInstance(HealthSystem instance)
    {
        Instance = instance;
    }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();

        switch (characterType)
        {
            case CHARACTER.Player:
                InitHealth();
                break;
            case CHARACTER.EnemyA:
                int enemyAId = GetComponent<EnemyAController>().Id;
                var dataA = GameManager.Instance.GetEnemyAData(enemyAId);
                _maxHealth = dataA.health;
                _currentHealth = _maxHealth;
                _originalColor = dataA.color;
                break;
            case CHARACTER.EnemyB:
                int enemyBId = GetComponent<EnemyBController>().Id;
                var dataB = GameManager.Instance.GetEnemyBData(enemyBId);
                _maxHealth = dataB.health;
                _currentHealth = _maxHealth;
                _originalColor = dataB.color;
                break;
        }

        _currentHealth = _maxHealth;
    }

    void Die()
    {
        if (killVFXPrefab != null)
        {
            float killVFXLifetime = 1f;
            Quaternion spawnRot = Quaternion.Euler(-90f, 0f, 0f);
            GameObject vfxInstance = Instantiate(killVFXPrefab, transform.position, spawnRot);
            Vector3 scale = vfxInstance.transform.localScale;
            if (Random.value < 0.5f) scale.x *= -1;
            if (Random.value < 0.5f) scale.y *= -1;
            vfxInstance.transform.localScale = scale;
            Destroy(vfxInstance, killVFXLifetime);
        }

        switch (characterType)
        {
            case CHARACTER.Player:
                StartCoroutine(PlayerDeath());
                break;
            case CHARACTER.EnemyA:
                StartCoroutine(EnemyADeath());
                break;
            case CHARACTER.EnemyB:
                StartCoroutine(EnemyBDeath());
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

    IEnumerator PlayerDeath()
    {
        Vector2 respawnPosition = GameManager.Instance.GetLastCheckpointPosition();
        float respawnDelay = GameManager.Instance.GetPlayerData().respawnDelay;
        GameManager.Instance.StartCoroutine(GameManager.Instance.Respawn(respawnDelay, respawnPosition));

        GetComponent<PlayerController>().SetDead(true);

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("IsDying");

            yield return null;

            float deathAnimDuration = animator.GetCurrentAnimatorStateInfo(0).length;
            yield return new WaitForSeconds(deathAnimDuration);
        }

        Destroy(gameObject);
    }

    IEnumerator EnemyADeath()
    {
        GetComponent<EnemyAController>().SetDead(true);

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("IsDying");

            yield return null;

            float deathAnimDuration = animator.GetCurrentAnimatorStateInfo(0).length;
            yield return new WaitForSeconds(deathAnimDuration);
        }

        Destroy(gameObject);
    }

    IEnumerator EnemyBDeath()
    {
        GetComponent<EnemyBController>().SetDead(true);

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.gravityScale = 3f;

        Animator animator = GetComponent<Animator>();
        if (rb != null && animator != null)
        {
            animator.SetTrigger("IsDying");

            yield return new WaitForSeconds(0.1f);

            yield return new WaitUntil(() => Mathf.Abs(rb.linearVelocity.y) < 0.01f);

            if (animator != null)
                animator.SetTrigger("IsDeadOnGround");

            yield return null;

            float deathAnimDuration = animator.GetCurrentAnimatorStateInfo(0).length;
            yield return new WaitForSeconds(deathAnimDuration);
        }

        Destroy(gameObject);
    }

    public void TakeDamage(int damage)
    {
        int previousHealth = _currentHealth;
        _currentHealth -= damage;
        StartCoroutine(FlashRed());

        if (_currentHealth <= 0) Die();

        if (healthUI != null)
            healthUI.UpdateHealth(true, previousHealth, _currentHealth);
    }

    public void HealDamage(int amount)
    {
        int previousHealth = _currentHealth;
        _currentHealth = Mathf.Min(_currentHealth + amount, _maxHealth);

        if (healthUI != null)
            healthUI.UpdateHealth(false, previousHealth, _currentHealth);
    }

    public void InitHealth()
    {
        if (characterType == CHARACTER.Player)
        {
            _maxHealth = GameManager.Instance.GetPlayerData().health;
            _originalColor = GameManager.Instance.GetPlayerData().color;
            _respawnDelay = GameManager.Instance.GetPlayerData().respawnDelay;
        }

        _currentHealth = _maxHealth;

        if (healthUI != null)
        {
            healthUI.SetHealthPoint(_currentHealth);
            healthUI.UpdateHealth(false, _maxHealth, _currentHealth);
        }
    }
}
