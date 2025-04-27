using UnityEngine;

public struct CheckpointData
{
    private int _id;
    private Vector2 _position;

    public CheckpointData(int id, Vector2 position)
    {
        _id = id;
        _position = position;
    }

    public int ID
    {
        get { return _id; }
        set { _id = value; }
    }

    public Vector2 Position
    {
        get { return _position; }
        set { _position = value; }
    }
}
