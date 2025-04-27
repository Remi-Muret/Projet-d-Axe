using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyBombDatabase", menuName = "Scriptable Objects/EnemyBombDatabase", order = 5)]
public class EnemyBombDatabase : ScriptableObject
{
	[SerializeField] private List<EnemyBombData> _enemyBombData = new();

	public EnemyBombData GetEnemyBombData(int id)
	{
		id = Mathf.Clamp(id, 0, _enemyBombData.Count - 1);
		return _enemyBombData[id];
	}
}
