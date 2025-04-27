using System;
using UnityEngine;

[Serializable]
public class ItemData
{
    public enum ITEM
    {
        ItemA,
        ItemB,
        ItemC,
    }

    public ITEM itemType;
	public float scaleCoef;
	public Sprite sprite;
}
