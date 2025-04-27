using System.Collections;
using UnityEngine;

public class EnemyBController : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool _guiDebug;
    [SerializeField] private bool _showGizmos;

    [Header("Set up")]
    public int id;
    [SerializeField] private bool _reverseOrientation;

    [Header("Bomb")]
    [SerializeField] private GameObject _bombPrefab;

    [Header("Ground check")]
    [SerializeField] private LayerMask _groundLayer;

    private EnemyBData _data;
    private AudioSource _audioSource;
    private Animator _animator;
    private Collider2D _collider2D;
    private GameObject _player;
    private Rigidbody2D _rigidbody2D;
    private SpriteRenderer _spriteRenderer;

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

    void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _animator = GetComponent<Animator>();
        _collider2D = GetComponent<Collider2D>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        _data = GameManager.Instance.GetEnemyBData(id);
        _player = GameObject.FindWithTag("Player");

        _spriteRenderer.sprite = _data.sprite;
        transform.localScale = Vector3.one * _data.scaleCoef;

        _originalColor = _data.color;
        _startPosition = transform.position;
        _patrolTargetA = _startPosition + _data.patrolPointA;
        _patrolTargetB = _startPosition + _data.patrolPointB;

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
        if (distanceToPlayer <= _data.detectionRadius && angleToPlayer > _data.detectionMinAngle && angleToPlayer < _data.detectionMaxAngle)
            _playerDetected = true;
        else if (distanceToPlayer > _data.detectionRadius)
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
                _patrolPauseTimer = _data.waitDuration;
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

        yield return new WaitForSeconds(_data.bombCooldown);

        _canBomb = true;
    }

    void Move()
    {
        _rigidbody2D.linearVelocity = new Vector2(_direction * _data.moveSpeed, _rigidbody2D.linearVelocity.y);
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        PlayerData _playerData = GameManager.Instance.GetPlayerData();
        if (collider.CompareTag("Player Attack"))
        {
            HealthSystem healthSystem = GetComponent<HealthSystem>();
            if (healthSystem != null)
                healthSystem.TakeDamage(_playerData.meleeDamage);
        }
        if (collider.CompareTag("Player Projectile"))
        {
            HealthSystem healthSystem = GetComponent<HealthSystem>();
            if (healthSystem != null)
                healthSystem.TakeDamage(_playerData.projectileDamage);
        }
        if (collider.CompareTag("Player Bomb"))
        {
            HealthSystem healthSystem = GetComponent<HealthSystem>();
            if (healthSystem != null)
                healthSystem.TakeDamage(_playerData.bombDamage);
        }
        if (collider.CompareTag("Player Bomb Explosion"))
        {
            HealthSystem healthSystem = GetComponent<HealthSystem>();
            if (healthSystem != null)
                healthSystem.TakeDamage(_playerData.bombDamage);
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
        Vector2 pointA = startPos + (_data != null ? _data.patrolPointA : Vector2.left * 2f);
        Vector2 pointB = startPos + (_data != null ? _data.patrolPointB : Vector2.right * 2f);

        Gizmos.DrawWireSphere(pointA, 0.2f);
        Gizmos.DrawWireSphere(pointB, 0.2f);
        Gizmos.DrawLine(pointA, pointB);

        if (_data != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _data.detectionRadius);
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
