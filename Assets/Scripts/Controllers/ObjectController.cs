using UnityEngine;

public class ObjectController : MonoBehaviour
{
    [SerializeField] protected ObjectData _data;

    protected AudioSource _audioSource;
    protected Animator _animator;
    protected Collider2D _collider2D;
    protected Rigidbody2D _rigidbody2D;
    protected SpriteRenderer _spriteRenderer;

    protected virtual void Awake()
    {
        TryGetComponent(out _audioSource);
        TryGetComponent(out _animator);
        TryGetComponent(out _collider2D);
        TryGetComponent(out _rigidbody2D);
        TryGetComponent(out _spriteRenderer);
    }

    protected virtual void Start()
    {
        Init();
    }

    protected virtual void Init()
    {
        if (_data == null) return;

        name = _data.label;
        transform.localScale = Vector3.one * _data.scale;

        if (_rigidbody2D != null)
            _rigidbody2D.gravityScale = _data.gravity;

        if (_spriteRenderer != null)
        {
            if (_data.sprite != null)
                _spriteRenderer.sprite = _data.sprite;
        }

        _spriteRenderer.color = _data.color;
    }
}
