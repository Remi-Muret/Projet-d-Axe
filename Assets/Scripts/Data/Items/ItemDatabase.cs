using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Scriptable Objects/ItemDatabase", order = 2)]
public class ItemDatabase : ScriptableObject
{
	[SerializeField] private List<ItemData> itemDatas = new();

	public ItemData GetItemData(ItemData.ITEM type) => itemDatas.Find(itemDatas => itemDatas.itemType == type);
}
