using UnityEngine;

public class CheckpointTrigger : MonoBehaviour
{
    [SerializeField] private int _id;

    public bool IsTrigger { get; private set; }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !IsTrigger)
        {
            IsTrigger = true;
            Vector2 position = transform.position;
            CheckpointData newCheckpoint = new CheckpointData(_id, position);
            GameManager.Instance.RegisterCheckpoint(newCheckpoint);

            ItemController item = GetComponent<ItemController>();
            if (item != null)
                TimeSystem.Instance.AddTime(item.ItemData.recovery);
        }
    }
}
