using System.Collections;
using UnityEngine;

public class EnemyBController : EnemyController
{
    private EnemyBData _enemyBData;

    protected override void Init()
    {
        _data = GameManager.Instance.GetEnemyBData(_id);
        _enemyBData = GameManager.Instance.GetEnemyBData(_id);

        base.Init();

        _patrolTargetA = _startPosition + _enemyBData.patrolPointA;
        _patrolTargetB = _startPosition + _enemyBData.patrolPointB;
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        Move();
        Patrol();
        Animation();

        if (DetectPlayer() && _canAttack)
            StartCoroutine(Bomb());

    }

    public void SetId(int id)
    {
        _id = id;
    }

    bool DetectPlayer()
    {
        if (_player == null) return false;

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
        _canAttack = false;
        Instantiate(_bombPrefab, new Vector3(transform.position.x, transform.position.y, transform.position.z), Quaternion.identity);

        yield return new WaitForSeconds(_enemyBData.bombCooldown);

        _canAttack = true;
    }

    void Move()
    {
        _rigidbody2D.linearVelocity = new Vector2(_direction * _enemyBData.moveSpeed, _rigidbody2D.linearVelocity.y);
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

    protected void Animation()
    {
        if (_direction != 0f)
            _spriteRenderer.flipX = _direction < 0f;
    }

    void OnDrawGizmos()
    {
        if (!_showGizmos) return;

        Gizmos.color = Color.red;

        Vector2 startPos = Application.isPlaying ? _startPosition : (Vector2)transform.position;
        Vector2 pointA = startPos + (_enemyBData != null ? _enemyBData.patrolPointA : Vector2.left);
        Vector2 pointB = startPos + (_enemyBData != null ? _enemyBData.patrolPointB : Vector2.right);

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
        GUILayout.Label($"_canBomb = {_canAttack}");
        GUILayout.Label($"_isPaused = {_isPaused}");
        GUILayout.EndVertical();
    }
}
