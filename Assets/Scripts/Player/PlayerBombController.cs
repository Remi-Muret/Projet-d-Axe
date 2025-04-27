using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBombController : MonoBehaviour
{
    [SerializeField] private GameObject _explosionPrefab;

    private float explosionLifetime = 0.2f;
    private Collider2D col;

    void Awake()
    {
        col = GetComponent<Collider2D>();

        IgnoreCollisions();
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
            "Player", "Anchor", "Checkpoint", "Item"
        };
        foreach (string tag in ignoredTags)
        {
            GameObject[] objectsWithTag = GameObject.FindGameObjectsWithTag(tag);

            foreach (GameObject obj in objectsWithTag)
            {
                Collider2D targetCol = obj.GetComponent<Collider2D>();

                Physics2D.IgnoreCollision(col, targetCol, true);
            }
        }
    }
}
