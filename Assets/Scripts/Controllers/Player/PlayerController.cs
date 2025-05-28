using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : FigureController
{
    public PlayerController Instance { get; private set; }

    [Header("Wall Checkers")]
    [SerializeField] private Transform _wallCheckerRight;
    [SerializeField] private Transform _wallCheckerLeft;

    [Header("Attacks")]
    [SerializeField] private Collider2D _hitboxRight;
    [SerializeField] private Collider2D _hitboxLeft;

    private PlayerData _playerData;

    private Vector2 _wallCheckerBoxSize = new Vector2(0f, 1.5f);

    private float _horizontalInput;
    private float _verticalInput;
    private float _lastBombTime;
    private float _lastProjectileTime;

    private bool _isDirectionLocked;
    private bool _isJumpSquatting;
    private bool _isJumping;
    private bool _isWallSliding;
    private bool _isWallJumping;
    private bool _isChargingAttack;
    private bool _isAttacking;
    private bool _isSettingThrow;
    private bool _isThrowing;
    private bool _isInvulnerable;
    private bool _isKnockedBack;
    private bool _isDying;
    private bool _wallSlideCooldown;
    private bool _bombForm;
    private bool _meleeForm;
    private bool _projectileForm;

    private string _currentState;

    protected override void Awake()
    {
        base.Awake();

        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    protected override void Start()
    {
        Init();
        StartCoroutine(InvulnerabilityTime());
    }

    protected override void Init()
    {
        _data = GameManager.Instance.GetPlayerData();
        _playerData = GameManager.Instance.GetPlayerData();

        base.Init();

        _isInvulnerable = true;
    }

    void FixedUpdate()
    {
        if (_isInvulnerable)
            IgnoreEnemyCollisions();
        else
            RestoreEnemyCollisions();

        if (_isKnockedBack) return;

        Move();
    }

    void Update()
    {
        HandleInputs();
        Animation();
    }

    public void SetDead(bool dead)
    {
        _isDying = dead;
    }

    public bool CheckGroundCollision() => Physics2D.OverlapBox(_groundChecker.position, _groundCheckerBoxSize, 0f, _groundLayer);
    
    bool CheckWallCollisionRight() => Physics2D.OverlapBox(_wallCheckerRight.position, _wallCheckerBoxSize, 0f, _groundLayer);
    
    bool CheckWallCollisionLeft() => Physics2D.OverlapBox(_wallCheckerLeft.position, _wallCheckerBoxSize, 0f, _groundLayer);

    void HandleInputs()
    {
        if (_isChargingAttack || _isAttacking || _isDying) return;

        _horizontalInput = Input.GetAxis("Horizontal");
        _verticalInput = Input.GetAxis("Vertical");

        if (Input.GetButton("A") && _bombForm) StartCoroutine(Bomb(_playerData.bombAngle));
        if (Input.GetButtonDown("A") && _meleeForm) StartCoroutine(Attack());
        if (Input.GetButton("A") && _projectileForm) StartCoroutine(Projectile());
        if (Input.GetButtonDown("B")) StartCoroutine(JumpImpulse());
    }

    void Move()
    {
        if (_isDirectionLocked) return;

        if (CheckGroundCollision())
            _isWallSliding = false;

        if (CheckWallCollisionRight() && !CheckGroundCollision() && !_isJumping && !_wallSlideCooldown)
        {
            StartCoroutine(WallSlide());
            return;
        }
        if (CheckWallCollisionLeft() && !CheckGroundCollision() && !_isJumping && !_wallSlideCooldown)
        {
            StartCoroutine(WallSlide());
            return;
        }

        _rigidbody2D.gravityScale = _playerData.gravity;
        _rigidbody2D.linearVelocity = new Vector2(_horizontalInput * _playerData.moveSpeed, _rigidbody2D.linearVelocity.y);
    }

    IEnumerator JumpImpulse()
    {
        if (CheckGroundCollision())
        {
            _isDirectionLocked = true;
            _isJumpSquatting = true;
            _rigidbody2D.linearVelocity = new Vector2(0f, 0f);

            yield return new WaitForSeconds(_playerData.jumpSquatDuration);

            _isDirectionLocked = false;
            _isJumpSquatting = false;
            StartCoroutine(Jump());
        }
        if (CheckWallCollisionRight() && _isWallSliding && Input.GetKey(KeyCode.D))
        {
            _isWallSliding = false;
            _isWallJumping = true;
            StartCoroutine(WallJump(-_playerData.wallJumpForceX));
        }
        else if (CheckWallCollisionLeft() && _isWallSliding && Input.GetKey(KeyCode.A))
        {
            _isWallSliding = false;
            _isWallJumping = true;
            StartCoroutine(WallJump(_playerData.wallJumpForceX));
        }
    }

    IEnumerator Jump()
    {
        _isWallSliding = false;
        _isJumping = true;
        _rigidbody2D.linearVelocity = new Vector2(_rigidbody2D.linearVelocity.x, _playerData.jumpForce);

        PlaySound(_jumpSound);

        yield return new WaitForSeconds(_playerData.jumpDuration);

        _isJumping = false;
    }

    IEnumerator WallSlide()
    {
        _isWallSliding = true;
        _rigidbody2D.gravityScale = _playerData.wallSlideGravity;
        _rigidbody2D.linearVelocity = new Vector2(0f, 0f);

        yield return new WaitUntil(() => (CheckWallCollisionRight() && Input.GetKeyDown(KeyCode.A)) || (CheckWallCollisionLeft() && Input.GetKeyDown(KeyCode.D)));

        _isWallSliding = false;
        _wallSlideCooldown = true;
        _rigidbody2D.linearVelocity = new Vector2(_horizontalInput * _playerData.moveSpeed, _rigidbody2D.linearVelocity.y);

        yield return new WaitForSeconds(0.1f);
        _wallSlideCooldown = false;
    }

    IEnumerator WallJump(float forceX)
    {
        _isDirectionLocked = true;
        _rigidbody2D.gravityScale = _playerData.gravity;
        _rigidbody2D.linearVelocity = new Vector2(forceX, _playerData.wallJumpForceY);

        yield return new WaitForSeconds(0.1f);

        yield return new WaitUntil(() => CheckGroundCollision() || CheckWallCollisionRight() || CheckWallCollisionLeft());

        _isDirectionLocked = false;
        _isWallJumping = false;
    }

    IEnumerator Bomb(float angle)
    {
        if (_isSettingThrow || Time.time - _lastBombTime < _playerData.bombCooldown || _isKnockedBack || _isDying) yield break;

        _isDirectionLocked = true;
        _isSettingThrow = true;
        _isThrowing = true;

        if (CheckGroundCollision())
            _rigidbody2D.linearVelocity = new Vector2(0f, 0f);

        yield return new WaitForSeconds(0.25f);

        if (CheckGroundCollision())
            _rigidbody2D.linearVelocity = new Vector2(0f, 0f);

        float offsetX = _spriteRenderer.flipX ? -0.3f : 0.3f;

        _lastBombTime = Time.time;

        GameObject bomb = Instantiate(
            _bombPrefab,
            new Vector3(transform.position.x + offsetX, transform.position.y, transform.position.z),
            Quaternion.Euler(0f, 0f, angle)
        );

        Vector2 direction = new Vector2(Mathf.Cos(Mathf.Deg2Rad * angle), Mathf.Sin(Mathf.Deg2Rad * angle));

        if (_spriteRenderer.flipX)
        {
            angle = 180f - angle;
            direction.x *= -1;
        }

        Rigidbody2D _bombRigidbody2D = bomb.GetComponent<Rigidbody2D>();
        _bombRigidbody2D.AddForce(direction * _playerData.bombForce, ForceMode2D.Impulse);
        _isDirectionLocked = false;
        _isSettingThrow = false;
        _isThrowing = false;
    }

    IEnumerator Attack()
    {
        if (!_isChargingAttack && !_isAttacking)
        {
            _isWallSliding = false;
            _isDirectionLocked = true;
            _isChargingAttack = true;
            _rigidbody2D.gravityScale = 0f;
            _rigidbody2D.linearVelocity = new Vector2(0f, -0.5f);

            float holdTime = 0f;
            while (Input.GetButton("A"))
            {
                holdTime += Time.deltaTime * _playerData.attackChargeRate;
                if (holdTime >= _playerData.maxAttackDuration - _playerData.minAttackDuration)
                {
                    holdTime = _playerData.maxAttackDuration - _playerData.minAttackDuration;
                    break;
                }

                if (_isKnockedBack)
                {
                    _isChargingAttack = false;
                    _rigidbody2D.gravityScale = _playerData.gravity;
                    yield break;
                }
                yield return null;
            }

            float adjustedAttackDuration = _playerData.minAttackDuration + holdTime;

            _isChargingAttack = false;
            _isAttacking = true;
            _isInvulnerable = true;
            if (_hitboxRight != null && _hitboxLeft != null)
            {
                if (_spriteRenderer.flipX)
                {
                    _hitboxRight.enabled = false;
                    _hitboxLeft.enabled = true;
                    _rigidbody2D.linearVelocity = new Vector2(-_playerData.dashSpeed, 0f);
                }
                else
                {
                    _hitboxRight.enabled = true;
                    _hitboxLeft.enabled = false;
                    _rigidbody2D.linearVelocity = new Vector2(_playerData.dashSpeed, 0f);
                }
            }

            StartCoroutine(Colorize(Color.yellow, adjustedAttackDuration));
            PlaySound(_attackSound);

            yield return new WaitForSeconds(adjustedAttackDuration);
        
            _hitboxRight.enabled = false;
            _hitboxLeft.enabled = false;
            _isDirectionLocked = false;
            _isInvulnerable = false;
            _isAttacking = false;
            _rigidbody2D.gravityScale = _playerData.gravity;

            if (_spriteRenderer.flipX && Input.GetKey(KeyCode.D) || !_spriteRenderer.flipX && Input.GetKey(KeyCode.A))
                Input.ResetInputAxes();
        }
    }

    IEnumerator Projectile()
    {
        if (_isSettingThrow || Time.time - _lastProjectileTime < _playerData.projectileCooldown || _isKnockedBack || _isDying) yield break;

        _isDirectionLocked = true;
        _isSettingThrow = true;
        _isThrowing = true;

        if (CheckGroundCollision())
            _rigidbody2D.linearVelocity = new Vector2(0f, 0f);

        yield return new WaitForSeconds(0.25f);

        if (CheckGroundCollision())
            _rigidbody2D.linearVelocity = new Vector2(0f, 0f);

        float offsetX = _spriteRenderer.flipX ? -0.3f : 0.3f;

        _lastProjectileTime = Time.time;

        GameObject projectile = Instantiate(
            _projectilePrefab,
            new Vector3(transform.position.x + offsetX, transform.position.y, transform.position.z),
            Quaternion.identity
        );

        Vector2 direction = Vector2.right;
        SpriteRenderer sprite = projectile.GetComponent<SpriteRenderer>();
        if (_spriteRenderer.flipX)
        {
            direction = Vector2.left;
            sprite.flipX = true;
        }

        Rigidbody2D _projectileRigidbody2D = projectile.GetComponent<Rigidbody2D>();
        _projectileRigidbody2D.linearVelocity = direction * _playerData.projectileForce;
        _isDirectionLocked = false;
        _isSettingThrow = false;
        _isThrowing = false;
    }

    IEnumerator ApplyKnockback()
    {
        _isInvulnerable = true;
        _isKnockedBack = true;
        _isDirectionLocked = true;

        if (_spriteRenderer.flipX)
        {
            if (CheckWallCollisionRight())
                _rigidbody2D.linearVelocity = new Vector2(-_playerData.knockbackX, _playerData.knockbackY);
            else
                _rigidbody2D.linearVelocity = new Vector2(_playerData.knockbackX, _playerData.knockbackY);
        }
        else
        {
            if (CheckWallCollisionLeft())
                _rigidbody2D.linearVelocity = new Vector2(_playerData.knockbackX, _playerData.knockbackY);
            else
                _rigidbody2D.linearVelocity = new Vector2(-_playerData.knockbackX, _playerData.knockbackY);
        }

        PlaySound(_hitSound);

        yield return new WaitForSeconds(0.1f);

        yield return new WaitUntil(() => CheckGroundCollision());

        _isKnockedBack = false;
        _isDirectionLocked = false;

        StartCoroutine(InvulnerabilityTime());
    }

    IEnumerator InvulnerabilityTime()
    {
        StartCoroutine(Colorize(Color.yellow, _playerData.invulnerabilityTime));

        yield return new WaitForSeconds(_playerData.invulnerabilityTime);

        _isInvulnerable = false;
    }

    void IgnoreEnemyCollisions()
    {
        string[] ignoredTags = new string[] {
            "Enemy", "Enemy Attack"
        };
        foreach (string tag in ignoredTags)
        {
            GameObject[] objectsWithTag = GameObject.FindGameObjectsWithTag(tag);

            foreach (GameObject obj in objectsWithTag)
            {
                Collider2D objectCollider = obj.GetComponent<Collider2D>();
                Physics2D.IgnoreCollision(_collider2D, objectCollider, true);
            }
        }
    }

    void RestoreEnemyCollisions()
    {
        string[] ignoredTags = new string[] {
            "Enemy", "Enemy Attack"
        };
        foreach (string tag in ignoredTags)
        {
            GameObject[] objectsWithTag = GameObject.FindGameObjectsWithTag(tag);

            foreach (GameObject obj in objectsWithTag)
            {
                Collider2D objectCollider = obj.GetComponent<Collider2D>();
                Physics2D.IgnoreCollision(_collider2D, objectCollider, false);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        ItemController item = collider.GetComponent<ItemController>();
        HealthSystem healthSystem = GetComponent<HealthSystem>();

        if (item != null && item.Category == ItemDatabase.Category.Power)
        {
            switch (item.Id)
            {
                case 0:
                    _bombForm = true;
                    _meleeForm = false;
                    _projectileForm = false;
                    break;
                case 1:
                    _bombForm = false;
                    _meleeForm = true;
                    _projectileForm = false;
                    break;
                case 2:
                    _bombForm = false;
                    _meleeForm = false;
                    _projectileForm = true;
                    break;
            }

            Destroy(item.gameObject);
        }

        if (item != null && item.Category == ItemDatabase.Category.Life)
        {
            TimeSystem.Instance.AddTime(item.ItemData.recovery);
            Destroy(item.gameObject);
        }

        if (collider.CompareTag("Enemy Attack") && !_isInvulnerable)
        {
            if (healthSystem != null)
            {
                int damage = 0;
                if (item != null && item.Category == ItemDatabase.Category.EnemyAttack)
                {
                    if (item.Id == 0)
                        damage = item.ItemData.damage;
                    else
                        damage = 1;
                }
                else
                {
                    damage = 1;
                }

                healthSystem.TakeDamage(damage);
                StartCoroutine(ApplyKnockback());
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        HealthSystem healthSystem = GetComponent<HealthSystem>();
        if (collision.collider.CompareTag("Enemy") && !_isInvulnerable)
        {
            EnemyAController enemyA = collision.collider.GetComponent<EnemyAController>();
            if (enemyA != null)
            {
                EnemyAData enemyAData = GameManager.Instance.GetEnemyAData(enemyA.Id);
                if (healthSystem != null)
                    healthSystem.TakeDamage(enemyAData.meleeDamage);

                StartCoroutine(ApplyKnockback());
                return;
            }

            EnemyBController enemyB = collision.collider.GetComponent<EnemyBController>();
            if (enemyB != null)
            {
                EnemyBData enemyBData = GameManager.Instance.GetEnemyBData(enemyB.Id);
                if (healthSystem != null)
                    healthSystem.TakeDamage(enemyBData.meleeDamage);

                StartCoroutine(ApplyKnockback());
                return;
            }
        }
    }

    void Animation()
    {
        if (!_isWallJumping && _horizontalInput != 0f)
            _spriteRenderer.flipX = _horizontalInput < 0f;
        else if (_isWallJumping)
            _spriteRenderer.flipX = _rigidbody2D.linearVelocity.x < 0f;

        if (CheckGroundCollision())
        {
            float horizontalSpeed = Mathf.Abs(_rigidbody2D.linearVelocity.x);
            _animator.SetFloat("Speed", horizontalSpeed);
        }

        if (_isJumpSquatting)
            _animator.SetBool("IsJumpSquatting", true);
        else
            _animator.SetBool("IsJumpSquatting", false);

        if (!CheckGroundCollision() && !_isWallSliding)
            _animator.SetBool("IsInAir", true);
        else
            _animator.SetBool("IsInAir", false);

        if (CheckWallCollisionRight() && !CheckGroundCollision() && !_isJumping)
        {
            _animator.SetBool("IsWallSliding", true);
            _spriteRenderer.flipX = false;
            return;
        }
        else if (CheckWallCollisionLeft() && !CheckGroundCollision() && !_isJumping)
        {
            _animator.SetBool("IsWallSliding", true);
            _spriteRenderer.flipX = true;
            return;
        }
        else
        {
            _animator.SetBool("IsWallSliding", false);
        }

        if (_isChargingAttack)
            _animator.SetBool("IsChargingAttack", true);
        else
            _animator.SetBool("IsChargingAttack", false);

        if (_isAttacking)
            _animator.SetBool("IsAttacking", true);
        else
            _animator.SetBool("IsAttacking", false);

        if (_isThrowing)
            _animator.SetBool("IsThrowing", true);
        else
            _animator.SetBool("IsThrowing", false);

        if (_isKnockedBack && !_isDying)
            _animator.SetBool("IsKnockedBack", true);
        else
            _animator.SetBool("IsKnockedBack", false);
    }

    IEnumerator Colorize(Color flashColor, float duration)
    {
        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = flashColor;

            yield return new WaitForSeconds(duration);

            _spriteRenderer.color = _data.color;
        }
    }

    void OnGUI()
    {
        if (!_guiDebug) return;

        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label(gameObject.name);
        GUILayout.Label($"_horizontalInput = {_horizontalInput}");
        GUILayout.Label($"_verticalInput = {_verticalInput}");
        GUILayout.Label($"_isDirectionLocked = {_isDirectionLocked}");
        GUILayout.Label($"_isJumping = {_isJumping}");
        GUILayout.Label($"_isWallSliding = {_isWallSliding}");
        GUILayout.Label($"_isWallJumping = {_isWallJumping}");
        GUILayout.Label($"_isInvulnerable = {_isInvulnerable}");
        GUILayout.Label($"_meleeForm = {_meleeForm}");
        GUILayout.Label($"_projectileForm = {_projectileForm}");
        GUILayout.Label($"_bombForm = {_bombForm}");
        GUILayout.EndVertical();
    }
}
