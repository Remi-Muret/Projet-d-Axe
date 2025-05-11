using System;
using UnityEngine;

[Serializable]
public class ItemData : ObjectData
{
    [Header("Item Prefab")]
    public GameObject Prefab;

    [Header("Coin Value")]
    public int value;
    
    [Header("Life Recovery")]
    public int recovery;
    
    [Header("Attack Damage")]
    public int damage;
}
