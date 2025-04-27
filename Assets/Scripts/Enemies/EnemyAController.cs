using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAController : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool _guiDebug;
    [SerializeField] private bool _showGizmos;

    [Header("Set up")]
    public int id;
    [SerializeField] private bool _reverseOrientation;

    [Header("Ground check")]
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private Transform _groundChecker;
    [SerializeField] private Vector2 _groundCheckerBoxSize;

    private EnemyAData _enemyAData;
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
    private float _jumpDirection;
    private float _chargeDirection;
    private float _patrolPauseTimer;
    private bool _playerDetected;
    private bool _canJump;
    private bool _canCharge;
    private bool _moveToPointA;
    private bool _isPaused;
    private bool _isCharging;
    private bool _isStunned;
    private bool _facingRight;
    private bool _facingLeft;

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
        _enemyAData = GameManager.Instance.GetEnemyAData(id);
        _player = GameObject.FindWithTag("Player");

        _spriteRenderer.sprite = _enemyAData.sprite;
        transform.localScale = Vector3.one * _enemyAData.scaleCoef;
        _rigidbody2D.gravityScale = _enemyAData.gravity;

        _originalColor = _enemyAData.color;
        _startPosition = transform.position;
        _patrolTargetA = _startPosition + _enemyAData.patrolPointA;
        _patrolTargetB = _startPosition + _enemyAData.patrolPointB;

        _moveToPointA = !_reverseOrientation;
        _canCharge = true;
    }

    void FixedUpdate()
    {
        if (_player == null)
        {
            _player = GameObject.FindGameObjectWithTag("Player");
            if (_player == null) return;
        }

        if (_spriteRenderer.flipX)
        {
            _facingRight = false;
            _facingLeft = true;
        }
        else
        {
            _facingRight = true;
            _facingLeft = false;
        }

        LockedDirection();
        Move();
        Jump();
        Patrol();
        Animation();

        if (DetectPlayer() && _canCharge)
            StartCoroutine(Charge()); 
    }
    
    void Update()
    {
        Animation();
    }

    bool CheckGroundCollision() => Physics2D.OverlapBox(_groundChecker.position, _groundCheckerBoxSize, 0f, _groundLayer);

    bool DetectPlayer()
    {
        Vector2 directionToPlayer = (_player.transform.position - transform.position).normalized;
        float distanceToPlayer = Vector2.Distance(transform.position, _player.transform.position);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, distanceToPlayer, _groundLayer);
        if (!_playerDetected && hit.collider != null && ((1 << hit.collider.gameObject.layer) & _groundLayer) != 0) return false;

        float verticaleDifference = Mathf.Abs(transform.position.y - _player.transform.position.y);
        if (verticaleDifference > 4.5f) return false;
        
        float minAngle = 0f, maxAngle = 0f;
        if (_facingRight)
        {
            minAngle = -25f;
            maxAngle = 45f;
        }
        else if (_facingLeft)
        {
            minAngle = 135f;
            maxAngle = -155f;
        }

        float angleToPlayer = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
        if (_facingRight)
        {
            if (distanceToPlayer <= _enemyAData.detectionRadius && angleToPlayer > minAngle && angleToPlayer < maxAngle)
                _playerDetected = true;
            else if (distanceToPlayer > _enemyAData.detectionRadius)
                _playerDetected = false;
        }
        else if (_facingLeft)
        {
            if (distanceToPlayer <= _enemyAData.detectionRadius && (angleToPlayer <= 180f && angleToPlayer > minAngle || angleToPlayer >= -180f && angleToPlayer < maxAngle))
                _playerDetected = true;
            else if (distanceToPlayer > _enemyAData.detectionRadius)
                _playerDetected = false;
        }

        return _playerDetected;
    }

    void Patrol()
    {
        if (_isCharging || _isStunned) return;

        if (DetectPlayer()) 
        {
            _direction = Mathf.Sign(_player.transform.position.x - transform.position.x);
            return;
        }

        Vector2 target = _moveToPointA ? _patrolTargetA : _patrolTargetB;
        float distance = Vector2.Distance(transform.position, target);

        if (!_isPaused)
        {
            _direction = Mathf.Sign(target.x - transform.position.x);

            if (distance <= 0.1f)
            {
                _rigidbody2D.linearVelocity = new Vector2(0f, _rigidbody2D.linearVelocity.y);
                _isPaused = true;
                _patrolPauseTimer = _enemyAData.waitDuration;
            }
        }
        else
        {
            _patrolPauseTimer -= Time.fixedDeltaTime;
            _rigidbody2D.linearVelocity = new Vector2(0f, _rigidbody2D.linearVelocity.y);

            if (_patrolPauseTimer <= 0f)
            {
                _isPaused = false;
                _moveToPointA = !_moveToPointA;
            }
        }
    }
    
    IEnumerator Charge()
    {
        _isCharging = true;
        _canCharge = false;

        if (_player != null)
            _direction = Mathf.Sign(_player.transform.position.x - transform.position.x);

        _chargeDirection = _direction > 0f ? 1f : -1f;

        yield return new WaitForSeconds(_enemyAData.chargeDuration);

        _isCharging = false;

        if (_player != null)
            _direction = Mathf.Sign(_player.transform.position.x - transform.position.x);

        yield return new WaitForSeconds(_enemyAData.chargeCooldown);

        _canCharge = true;
    }

    IEnumerator Hitstun()
    {
        _isStunned = true;
        _rigidbody2D.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(_enemyAData.hitstunDuration);

        _isStunned = false;
    }

    void LockedDirection()
    {
        bool wasGrounded = _canJump;
        _canJump = CheckGroundCollision();
        if (!_canJump && wasGrounded)
        {
            if (_direction > 0f)
                _jumpDirection = 1f;
            else if (_direction < 0f)
                _jumpDirection = -1f;
        }
    }

    void Move()
    {
        if (_isStunned) return;

        if (_isCharging)
            _rigidbody2D.linearVelocity = new Vector2(_chargeDirection * _enemyAData.chargeSpeed, _rigidbody2D.linearVelocity.y);
        else
            _rigidbody2D.linearVelocity = new Vector2(_direction * _enemyAData.moveSpeed, _rigidbody2D.linearVelocity.y);
    }

    void Jump()
    {
        if (_isCharging || _isStunned) return;

        Vector2 forwardDirection = new Vector2(_direction, 0f);
        Vector2 raycastOrigin = new Vector2(transform.position.x, transform.position.y - 1.5f);
        RaycastHit2D hitForward = Physics2D.Raycast(raycastOrigin, forwardDirection, 1f, _groundLayer);
        if (hitForward.collider != null && _canJump)
            _rigidbody2D.linearVelocity = new Vector2(_jumpDirection * _enemyAData.moveSpeed, _enemyAData.jumpForce);
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
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        PlayerData _playerData = GameManager.Instance.GetPlayerData();
        if (collision.collider.CompareTag("Player"))
        {
            StartCoroutine(Hitstun()); 
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
        Vector2 pointA = startPos + (_enemyAData != null ? _enemyAData.patrolPointA : Vector2.left * 2f);
        Vector2 pointB = startPos + (_enemyAData != null ? _enemyAData.patrolPointB : Vector2.right * 2f);

        Gizmos.DrawWireSphere(pointA, 0.2f);
        Gizmos.DrawWireSphere(pointB, 0.2f);
        Gizmos.DrawLine(pointA, pointB);

        if (_enemyAData != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _enemyAData.detectionRadius);
            Gizmos.color = Color.green;
            float verticalLimit = 4.5f;
            float radius = _enemyAData.detectionRadius;

            Vector2 topStart = new Vector2(transform.position.x - radius, transform.position.y + verticalLimit);
            Vector2 topEnd = new Vector2(transform.position.x + radius, transform.position.y + verticalLimit);
            Gizmos.DrawLine(topStart, topEnd);

            Vector2 bottomStart = new Vector2(transform.position.x - radius, transform.position.y - verticalLimit);
            Vector2 bottomEnd = new Vector2(transform.position.x + radius, transform.position.y - verticalLimit);
            Gizmos.DrawLine(bottomStart, bottomEnd);
        }
    }

    void OnGUI()
    {
        if (!_guiDebug) return;

        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label(gameObject.name);
        GUILayout.Label($"_direction = {_direction}");
        GUILayout.Label($"_jumpDirection = {_jumpDirection}");
        GUILayout.Label($"_chargeDirection = {_chargeDirection}");
        GUILayout.Label($"_playerDetected = {_playerDetected}");
        GUILayout.Label($"_canJump = {_canJump}");
        GUILayout.Label($"_canCharge = {_canCharge}");
        GUILayout.Label($"_isPaused = {_isPaused}");
        GUILayout.Label($"_isCharging = {_isCharging}");
        GUILayout.Label($"_isStunned = {_isStunned}");
        GUILayout.EndVertical();
    }
}
