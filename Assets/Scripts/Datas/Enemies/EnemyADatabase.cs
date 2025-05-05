using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyADatabase", menuName = "Scriptable Objects/EnemyADatabase", order = 3)]
public class EnemyADatabase : ScriptableObject
{
	[SerializeField] private List<EnemyAData> _enemyAData = new();

	public EnemyAData GetEnemyAData(int id)
	{
		id = Mathf.Clamp(id, 0, _enemyAData.Count - 1);
		return _enemyAData[id];
	}
}
