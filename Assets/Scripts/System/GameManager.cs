using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private PlayerData _playerData;
    [SerializeField] private EnemyADatabase _enemyADatabase;
    [SerializeField] private EnemyBDatabase _enemyBDatabase;
    [SerializeField] private EnemyBombDatabase _enemyBombDatabase;
    [SerializeField] private ItemDatabase _itemDatabase;
    [SerializeField] private GameObject _playerPrefab;

    private CheckpointData _currentCheckpoint;
    private TimeSystem _currentTime;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    public PlayerData GetPlayerData() => _playerData;

    public EnemyAData GetEnemyAData(int id) => _enemyADatabase.GetEnemyAData(id);

    public EnemyBData GetEnemyBData(int id) => _enemyBDatabase.GetEnemyBData(id);

    public EnemyBombData GetEnemyBombData(int id) => _enemyBombDatabase.GetEnemyBombData(id);

    public ItemDatabase GetItemData() => _itemDatabase;

    public void RegisterCheckpoint(CheckpointData checkpoint)
    {
        if (checkpoint.ID > _currentCheckpoint.ID)
            _currentCheckpoint = checkpoint;
    }

    public Vector2 GetLastCheckpointPosition() => _currentCheckpoint.Position;

    public IEnumerator RespawnPlayerAfterDelay(float delay, Vector2 position)
    {
        yield return new WaitForSeconds(delay);

        TimeSystem timeSystem = FindFirstObjectByType<TimeSystem>();
        if (timeSystem != null)
            timeSystem.ResetTimer();

        Instantiate(_playerPrefab, position, Quaternion.identity);
    }
}
