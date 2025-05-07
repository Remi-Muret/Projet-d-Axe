using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAController : EnemyController
{
    private EnemyAData _enemyAData;

    private float _chargeDirection;
    private bool _isCharging;
    private bool _isStunned;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        Init();

        _patrolTargetA = _startPosition + _enemyAData.patrolPointA;
        _patrolTargetB = _startPosition + _enemyAData.patrolPointB;
    }

    protected override void Init()
    {
        _data = GameManager.Instance.GetEnemyAData(_id);
        _enemyAData = GameManager.Instance.GetEnemyAData(_id);
        
        base.Init();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        LockedDirection();
        Move();
        Jump();

        if (_player == null)
        {
            _player = GameObject.FindGameObjectWithTag("Player");
            if (_player == null) return;
        }

        Patrol();

        if (DetectPlayer() && _canAttack)
            StartCoroutine(Charge()); 
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
        _canAttack = false;

        if (_player != null)
            _direction = Mathf.Sign(_player.transform.position.x - transform.position.x);

        _chargeDirection = _direction > 0f ? 1f : -1f;

        yield return new WaitForSeconds(_enemyAData.chargeDuration);

        _isCharging = false;

        if (_player != null)
            _direction = Mathf.Sign(_player.transform.position.x - transform.position.x);

        yield return new WaitForSeconds(_enemyAData.chargeCooldown);

        _canAttack = true;
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
        HealthSystem healthSystem = GetComponent<HealthSystem>();

        if (collider.CompareTag("Player Attack"))
        {
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

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
            StartCoroutine(Hitstun()); 
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
        GUILayout.Label($"_canAttack = {_canAttack}");
        GUILayout.Label($"_isPaused = {_isPaused}");
        GUILayout.Label($"_isCharging = {_isCharging}");
        GUILayout.Label($"_isStunned = {_isStunned}");
        GUILayout.EndVertical();
    }
}
