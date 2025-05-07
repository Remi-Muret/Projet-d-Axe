using UnityEngine;

[CreateAssetMenu(fileName = "PlayerDatabase", menuName = "Scriptable Objects/PlayerDatabase", order = 1)]
public class PlayerDatabase : ScriptableObject
{
    [SerializeField] private PlayerData _playerData;

    public PlayerData GetData() => _playerData;
}
