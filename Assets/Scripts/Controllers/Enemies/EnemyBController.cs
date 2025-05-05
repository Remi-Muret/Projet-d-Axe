using System.Collections;
using UnityEngine;

public class EnemyBController : ObjectController
{
    [Header("Debug")]
    [SerializeField] private bool _guiDebug;
    [SerializeField] private bool _showGizmos;

    [Header("Set up")]
    [SerializeField] private int _id;
    [SerializeField] private bool _reverseOrientation;

    public int Id => _id;

    [Header("Bomb")]
    [SerializeField] private GameObject _bombPrefab;

    [Header("Ground check")]
    [SerializeField] private LayerMask _groundLayer;

    private EnemyBData _enemyBData;
    private GameObject _player;

    private Color _originalColor;
    private Vector2 _startPosition;
    private Vector2 _patrolTargetA;
    private Vector2 _patrolTargetB;

    private float _direction;
    private float _patrolPauseTimer;
    private bool _playerDetected;
    private bool _canBomb;
    private bool _moveToPointA;
    private bool _isPaused;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        Init();
    }

    protected override void Init()
    {
        _enemyBData = GameManager.Instance.GetEnemyBData(_id);
        _player = GameObject.FindWithTag("Player");

        base.Init();

        _startPosition = transform.position;
        _patrolTargetA = _startPosition + _enemyBData.patrolPointA;
        _patrolTargetB = _startPosition + _enemyBData.patrolPointB;

        _moveToPointA = !_reverseOrientation;
        _canBomb = true;
    }

    void FixedUpdate()
    {
        Move();
        Patrol();
        Animation();

        if (_player == null)
        {
            _player = GameObject.FindGameObjectWithTag("Player");
            if (_player == null) return;
        }

        if (DetectPlayer() && _canBomb)
            StartCoroutine(Bomb()); 
    }
    
    void Update()
    {
        Animation();
    }

    bool DetectPlayer()
    {
        Vector2 directionToPlayer = (_player.transform.position - transform.position).normalized;
        float distanceToPlayer = Vector2.Distance(transform.position, _player.transform.position);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, distanceToPlayer, _groundLayer);
        if (!_playerDetected && hit.collider != null && ((1 << hit.collider.gameObject.layer) & _groundLayer) != 0) return false;

        float verticaleDifference = Mathf.Abs(transform.position.y - _player.transform.position.y);
        if (verticaleDifference > 4.5f) return false;

        float angleToPlayer = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
        if (distanceToPlayer <= _enemyBData.detectionRadius && angleToPlayer > _enemyBData.detectionMinAngle && angleToPlayer < _enemyBData.detectionMaxAngle)
            _playerDetected = true;
        else if (distanceToPlayer > _enemyBData.detectionRadius)
            _playerDetected = false;

        return _playerDetected;
    }

    void Patrol()
    {
        Vector2 target = _moveToPointA ? _patrolTargetA : _patrolTargetB;
        float distance = Vector2.Distance(transform.position, target);

        if (!_isPaused)
        {
            _direction = Mathf.Sign(target.x - transform.position.x);

            if (distance <= 0.1f)
            {
                _rigidbody2D.linearVelocity = new Vector2(0f, 0f);
                _isPaused = true;
                _patrolPauseTimer = _enemyBData.waitDuration;
            }
        }
        else
        {
            _patrolPauseTimer -= Time.fixedDeltaTime;
            _rigidbody2D.linearVelocity = new Vector2(0f, 0f);

            if (_patrolPauseTimer <= 0f)
            {
                _isPaused = false;
                _moveToPointA = !_moveToPointA;
            }
        }
    }

    IEnumerator Bomb()
    {
        _canBomb = false;
        Instantiate(_bombPrefab, new Vector3(transform.position.x, transform.position.y, transform.position.z), Quaternion.identity);

        yield return new WaitForSeconds(_enemyBData.bombCooldown);

        _canBomb = true;
    }

    void Move()
    {
        _rigidbody2D.linearVelocity = new Vector2(_direction * _enemyBData.moveSpeed, _rigidbody2D.linearVelocity.y);
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        PlayerData _playerData = GameManager.Instance.GetPlayerData();
        if (collider.CompareTag("Player Attack"))
        {
            HealthSystem healthSystem = GetComponent<HealthSystem>();
            if (healthSystem != null)
            {
                ItemController item = collider.GetComponent<ItemController>();
                if (item != null && item.Category == ItemDatabase.Category.PlayerAttack)
                {
                    int damage = 0;
                    switch (item.Id)
                    {
                        case 0:
                            damage = _playerData.bombDamage;
                            break;
                        case 1:
                            damage = _playerData.projectileDamage;
                            break;
                        default:
                            damage = _playerData.meleeDamage;
                            break;
                    }
                    healthSystem.TakeDamage(damage);
                }
                else
                {
                    healthSystem.TakeDamage(_playerData.meleeDamage);
                }
            }
        }
    }

    void Animation()
    {
        if (_direction != 0f)
            _spriteRenderer.flipX = _direction < 0f;
    }

    void PlaySound(AudioClip clip)
    {
        if (_audioSource != null && clip != null)
            _audioSource.PlayOneShot(clip);
    }

    void OnDrawGizmos()
    {
        if (!_showGizmos) return;

        Gizmos.color = Color.red;

        Vector2 startPos = Application.isPlaying ? _startPosition : (Vector2)transform.position;
        Vector2 pointA = startPos + (_enemyBData != null ? _enemyBData.patrolPointA : Vector2.left * 2f);
        Vector2 pointB = startPos + (_enemyBData != null ? _enemyBData.patrolPointB : Vector2.right * 2f);

        Gizmos.DrawWireSphere(pointA, 0.2f);
        Gizmos.DrawWireSphere(pointB, 0.2f);
        Gizmos.DrawLine(pointA, pointB);

        if (_enemyBData != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _enemyBData.detectionRadius);
        }
    }

    void OnGUI()
    {
        if (!_guiDebug) return;

        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label(gameObject.name);
        GUILayout.Label($"_direction = {_direction}");
        GUILayout.Label($"_playerDetected = {_playerDetected}");
        GUILayout.Label($"_canBomb = {_canBomb}");
        GUILayout.Label($"_isPaused = {_isPaused}");
        GUILayout.EndVertical();
    }
}
