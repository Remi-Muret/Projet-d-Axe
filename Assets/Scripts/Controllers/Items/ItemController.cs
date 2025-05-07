using UnityEngine;

public class ItemController : ObjectController
{
    [field: SerializeField] public ItemDatabase.Category Category { get; private set; }
    [SerializeField] private int _id;

    public int Id => _id;

    [SerializeField] private GameObject playerExplosionPrefab;
    [SerializeField] private GameObject enemyExplosionPrefab;
    [SerializeField] private float explosionDuration;

    private ItemData _itemData;

    protected override void Awake()
    {
        base.Awake();
        if (gameObject.tag == "Player Attack")
            IgnorePlayerCollisions();
        if (gameObject.tag == "Enemy Attack")
            IgnoreEnemyCollisions();
    }

    protected override void Start()
    {
        Init();
    }

    protected override void Init()
    {
        _data = GameManager.Instance.GetItemData(Category, _id);
        _itemData = GameManager.Instance.GetItemData(Category, _id);

        base.Init();
    }

    void IgnorePlayerCollisions()
    {
        string[] ignoredTags = { "Player", "Anchor", "Checkpoint", "Item" };
        Collider2D col = GetComponent<Collider2D>();

        foreach (string tag in ignoredTags)
        {
            GameObject[] objectsWithTag = GameObject.FindGameObjectsWithTag(tag);

            foreach (GameObject obj in objectsWithTag)
            {
                Collider2D targetCol = obj.GetComponent<Collider2D>();
                if (targetCol != null)
                    Physics2D.IgnoreCollision(col, targetCol, true);
            }
        }
    }

    void IgnoreEnemyCollisions()
    {
        string[] ignoredTags = { "Enemy", "Anchor", "Checkpoint", "Item" };
        Collider2D col = GetComponent<Collider2D>();

        foreach (string tag in ignoredTags)
        {
            GameObject[] objectsWithTag = GameObject.FindGameObjectsWithTag(tag);

            foreach (GameObject obj in objectsWithTag)
            {
                Collider2D targetCol = obj.GetComponent<Collider2D>();
                if (targetCol != null)
                    Physics2D.IgnoreCollision(col, targetCol, true);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if ((Category == ItemDatabase.Category.PlayerAttack && Id == 0) ||
            (Category == ItemDatabase.Category.EnemyAttack && Id == 0))
            SpawnExplosion();

        if (Category == ItemDatabase.Category.PlayerAttack ||
            Category == ItemDatabase.Category.EnemyAttack)
            Destroy(gameObject);
    }

    void SpawnExplosion()
    {
        GameObject explosionPrefab = null;

        if (gameObject.tag == "Player Attack")
            explosionPrefab = playerExplosionPrefab;
        else if (gameObject.tag == "Enemy Attack")
            explosionPrefab = enemyExplosionPrefab;

        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(explosion, explosionDuration);
        }
    }
}
