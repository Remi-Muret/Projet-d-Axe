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
        public int CheckpointId;
        public Vector2 Position;
    }

    [SerializeField] private PlayerDatabase _playerDatabase;
    [SerializeField] private EnemyADatabase _enemyADatabase;
    [SerializeField] private EnemyBDatabase _enemyBDatabase;
    [SerializeField] private ItemDatabase _itemDatabase;
    [SerializeField] private GameObject _playerPrefab;

    private CheckpointData _currentCheckpoint;
    private TimeSystem _currentTime;
    private Transform _enemiesParent;
    private Transform _itemsParent;

    private List<SpawnInfo> _spawnData = new();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        _enemiesParent = GameObject.Find("- Enemies -")?.transform;
        if (_enemiesParent == null)
            _enemiesParent = new GameObject("- Enemies -").transform;

        _itemsParent = GameObject.Find("- Items -")?.transform;
        if (_itemsParent == null)
            _itemsParent = new GameObject("- Items -").transform;
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

        GameObject player = Instantiate(_playerPrefab, position, Quaternion.identity);

        HealthSystem healthSystem = player.GetComponent<HealthSystem>();
        HealthSystem.SetInstance(healthSystem);

        healthSystem.healthUI = FindFirstObjectByType<HealthUI>();
        healthSystem.InitHealth();

        FindFirstObjectByType<MainUIController>().SetPlayerHealthSystem(healthSystem);
    }

    void RegisterObjects()
    {
        _spawnData.Clear();

        foreach (var enemyA in FindObjectsByType<EnemyAController>(FindObjectsSortMode.None))
        {
            _spawnData.Add(new SpawnInfo
            {
                objectType = SpawnInfo.Object.EnemyA,
                Id = enemyA.Id,
                Position = enemyA.transform.position
            });
        }

        foreach (var enemyB in FindObjectsByType<EnemyBController>(FindObjectsSortMode.None))
        {
            _spawnData.Add(new SpawnInfo
            {
                objectType = SpawnInfo.Object.EnemyB,
                Id = enemyB.Id,
                Position = enemyB.transform.position
            });
        }

        foreach (var item in FindObjectsByType<ItemController>(FindObjectsSortMode.None))
        {
            var checkpoint = item.GetComponent<CheckpointTrigger>();
            _spawnData.Add(new SpawnInfo
            {
                objectType = SpawnInfo.Object.Item,
                Id = item.Id,
                Category = item.Category,
                Position = item.transform.position,
                CheckpointId = checkpoint != null ? checkpoint.Id : -1
            });
        }
    }

    public void RespawnObjects()
    {
        foreach (var enemyA in FindObjectsByType<EnemyAController>(FindObjectsSortMode.None))
            Destroy(enemyA.gameObject);

        foreach (var enemyB in FindObjectsByType<EnemyBController>(FindObjectsSortMode.None))
            Destroy(enemyB.gameObject);

        foreach (var item in FindObjectsByType<ItemController>(FindObjectsSortMode.None))
            Destroy(item.gameObject);

        foreach (var info in _spawnData)
        {
            GameObject prefab = null;
            GameObject instance = null;

            switch (info.objectType)
            {
                case SpawnInfo.Object.EnemyA:
                    prefab = _enemyADatabase.GetData(info.Id).Prefab;
                    instance = Instantiate(prefab, info.Position, Quaternion.identity, _enemiesParent);
                    var controllerA = instance.GetComponent<EnemyAController>();
                    if (controllerA != null)
                        controllerA.SetId(info.Id);
                    break;
                case SpawnInfo.Object.EnemyB:
                    prefab = _enemyBDatabase.GetData(info.Id).Prefab;
                    instance = Instantiate(prefab, info.Position, Quaternion.identity, _enemiesParent);
                    var controllerB = instance.GetComponent<EnemyBController>();
                    if (controllerB != null)
                        controllerB.SetId(info.Id);
                    break;
                case SpawnInfo.Object.Item:
                    prefab = _itemDatabase.GetData(info.Category, info.Id).Prefab;
                    instance = Instantiate(prefab, info.Position, Quaternion.identity, _itemsParent);
                    var itemController = instance.GetComponent<ItemController>();
                    if (itemController != null)
                        itemController.SetId(info.Id);
                    var checkpoint = instance.GetComponent<CheckpointTrigger>();
                    if (checkpoint != null)
                        checkpoint.SetId(info.CheckpointId);
                    break;
            }
        }
    }
}
