using UnityEngine;

public class EnemyController : FigureController
{
    [Header("Set up")]
    [SerializeField] protected int _id;
    [SerializeField] protected bool _reverseOrientation;
    public int Id => _id;

    protected GameObject _player;

    protected Vector2 _startPosition;
    protected Vector2 _patrolTargetA;
    protected Vector2 _patrolTargetB;

    protected float _direction;
    protected float _patrolPauseTimer;
    protected float _jumpDirection;

    protected bool _playerDetected;
    protected bool _facingRight;
    protected bool _facingLeft;
    protected bool _canJump;
    protected bool _canAttack;
    protected bool _moveToPointA;
    protected bool _isPaused;

    protected override void Init()
    {
        base.Init();

        _startPosition = transform.position;

        _canAttack = true;
        _moveToPointA = !_reverseOrientation;

        _player = GameObject.FindGameObjectWithTag("Player");
    }

    protected virtual void FixedUpdate()
    {
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

        if (_player == null)
        {
            _player = GameObject.FindGameObjectWithTag("Player");
            if (_player == null) return;
        }
    }
}
