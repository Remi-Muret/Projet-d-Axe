using UnityEngine;

public class CheckpointTrigger : MonoBehaviour
{
    [SerializeField] private int _id;

    private bool isTrigger;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isTrigger)
        {
            isTrigger = true;
            Vector2 position = transform.position;
            CheckpointData newCheckpoint = new CheckpointData(_id, position);
            GameManager.Instance.RegisterCheckpoint(newCheckpoint);
        }
    }
}
