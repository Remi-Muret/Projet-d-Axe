using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Scriptable Objects/ItemDatabase", order = 2)]
public class ItemDatabase : ScriptableObject
{
    public enum Category
    {
        Power,
        Coin,
        Life,
        PlayerAttack,
        EnemyAttack,
    }

    [System.Serializable]
    public class ItemCategoryGroup
    {
        public string label;
        public Category category;
        public List<ObjectData> items = new();
    }

    [SerializeField] private List<ItemCategoryGroup> _categoryGroups = new();

    public List<ObjectData> GetItemsByCategory(Category category)
    {
        var group = _categoryGroups.Find(g => g.category == category);
        return group != null ? group.items : null;
    }

    public ObjectData GetData(Category category, int id)
    {
        var group = _categoryGroups.Find(g => g.category == category);
        return (group != null && id >= 0 && id < group.items.Count) ? group.items[id] : null;
    }
}
