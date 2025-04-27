using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerProjectileController : MonoBehaviour
{
    private Collider2D col;

    void Start()
    {
        col = GetComponent<Collider2D>();

        IgnoreCollisions();
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
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
