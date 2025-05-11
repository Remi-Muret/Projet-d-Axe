using UnityEngine;

public struct CheckpointData
{
    public int Id { get; }
    public Vector2 Position { get; }

    public CheckpointData(int id, Vector2 position)
    {
        Id = id;
        Position = position;
    }
}
