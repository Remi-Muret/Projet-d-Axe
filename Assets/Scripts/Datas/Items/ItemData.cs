using System;
using UnityEngine;

[Serializable]
public class ItemData : ObjectData
{
    [Header("Item Prefab")]
    public GameObject Prefab;
    
    [Header("Life Recovery")]
    public int recovery;
    
    [Header("Attack Damage")]
    public int damage;
}
