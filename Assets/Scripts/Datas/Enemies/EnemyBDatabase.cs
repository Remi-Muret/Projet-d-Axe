using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyBDatabase", menuName = "Scriptable Objects/EnemyBDatabase", order = 4)]
public class EnemyBDatabase : ScriptableObject
{
	[SerializeField] private List<EnemyBData> enemyBData = new();

	public EnemyBData GetData(int id)
	{
		id = Mathf.Clamp(id, 0, enemyBData.Count - 1);
		return enemyBData[id];
	}
}
