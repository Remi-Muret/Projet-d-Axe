using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBombController : MonoBehaviour
{
    public int id;
    [SerializeField] private GameObject _explosionPrefab;

    private EnemyBombData _data;
    private Collider2D _collider2D;
    private Rigidbody2D _rigidbody2D;
    private SpriteRenderer _spriteRenderer;

    private float explosionLifetime = 0.2f;

    void Awake()
    {
        _collider2D = GetComponent<Collider2D>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        IgnoreCollisions();
    }

    void Start()
    {
        _data = GameManager.Instance.GetEnemyBombData(id);

        transform.localScale = Vector3.one * _data.scaleCoef;
        _rigidbody2D.gravityScale = _data.gravity;

        if (_data.sprite != null)
            _spriteRenderer.sprite = _data.sprite;
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        GameObject explosionInstance = Instantiate(_explosionPrefab, transform.position, Quaternion.identity);

        Destroy(explosionInstance, explosionLifetime);
        Destroy(gameObject);
    }

    void IgnoreCollisions()
    {
        string[] ignoredTags = new string[] {
            "Enemy A", "Enemy B", "Anchor", "Checkpoint", "Item"
        };
        foreach (string tag in ignoredTags)
        {
            GameObject[] objectsWithTag = GameObject.FindGameObjectsWithTag(tag);

            foreach (GameObject obj in objectsWithTag)
            {
                Collider2D targetCol = obj.GetComponent<Collider2D>();

                Physics2D.IgnoreCollision(_collider2D, targetCol, true);
            }
        }
    }
}
