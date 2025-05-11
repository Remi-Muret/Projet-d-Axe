using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private class SpawnInfo
    {
        public enum Object
        {
            EnemyA,
            EnemyB,
            Item
        }

        public Object objectType;
        public ItemDatabase.Category Category;
        public int Id;
        public Vector2 Position;
    }

    [SerializeField] private PlayerDatabase _playerDatabase;
    [SerializeField] private EnemyADatabase _enemyADatabase;
    [SerializeField] private EnemyBDatabase _enemyBDatabase;
    [SerializeField] private ItemDatabase _itemDatabase;
    [SerializeField] private GameObject _playerPrefab;

    private CheckpointData _currentCheckpoint;
    private TimeSystem _currentTime;

    private List<SpawnInfo> _spawnData = new();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        RegisterObjects();
    }

    public PlayerData GetPlayerData() => _playerDatabase.GetData();

    public EnemyAData GetEnemyAData(int id) => _enemyADatabase.GetData(id);

    public EnemyBData GetEnemyBData(int id) => _enemyBDatabase.GetData(id);

    public ItemData GetItemData(ItemDatabase.Category category, int id) => _itemDatabase.GetData(category, id);

    public Vector2 GetLastCheckpointPosition() => _currentCheckpoint.Position;

    public void RegisterCheckpoint(CheckpointData checkpoint)
    {
        if (checkpoint.Id > _currentCheckpoint.Id)
            _currentCheckpoint = checkpoint;
    }

    public IEnumerator Respawn(float delay, Vector2 position)
    {
        yield return new WaitForSeconds(delay);

        RespawnObjects();
        TimeSystem.Instance.ResetTimer();
        Instantiate(_playerPrefab, position, Quaternion.identity);
    }

    void RegisterObjects()
    {
        _spawnData.Clear();

        foreach (var enemyA in FindObjectsOfType<EnemyAController>())
        {
            _spawnData.Add(new SpawnInfo
            {
                objectType = SpawnInfo.Object.EnemyA,
                Id = enemyA.Id,
                Position = enemyA.transform.position
            });
        }

        foreach (var enemyB in FindObjectsOfType<EnemyBController>())
        {
            _spawnData.Add(new SpawnInfo
            {
                objectType = SpawnInfo.Object.EnemyB,
                Id = enemyB.Id,
                Position = enemyB.transform.position
            });
        }

        foreach (var item in FindObjectsOfType<ItemController>())
        {
            _spawnData.Add(new SpawnInfo
            {
                objectType = SpawnInfo.Object.Item,
                Id = item.Id,
                Category = item.Category,
                Position = item.transform.position
            });
        }
    }

    void RespawnObjects()
    {
        foreach (var enemyA in FindObjectsOfType<EnemyAController>())
            Destroy(enemyA.gameObject);

        foreach (var enemyB in FindObjectsOfType<EnemyBController>())
            Destroy(enemyB.gameObject);

        foreach (var item in FindObjectsOfType<ItemController>())
            Destroy(item.gameObject);

        foreach (var info in _spawnData)
        {
            GameObject prefab = null;
            GameObject instance = null;

            switch (info.objectType)
            {
                case SpawnInfo.Object.EnemyA:
                    prefab = _enemyADatabase.GetData(info.Id).Prefab;
                    instance = Instantiate(prefab, info.Position, Quaternion.identity);
                    var controllerA = instance.GetComponent<EnemyAController>();
                    if (controllerA != null)
                        controllerA.SetId(info.Id);
                    break;
                case SpawnInfo.Object.EnemyB:
                    prefab = _enemyBDatabase.GetData(info.Id).Prefab;
                    instance = Instantiate(prefab, info.Position, Quaternion.identity);
                    var controllerB = instance.GetComponent<EnemyBController>();
                    if (controllerB != null)
                        controllerB.SetId(info.Id);
                    break;
                case SpawnInfo.Object.Item:
                    prefab = _itemDatabase.GetData(info.Category, info.Id).Prefab;
                    instance = Instantiate(prefab, info.Position, Quaternion.identity);
                    var itemController = instance.GetComponent<ItemController>();
                    if (itemController != null)
                        itemController.SetId(info.Id);
                    break;
            }
        }
    }
}
