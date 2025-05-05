using System;
using UnityEngine;

[Serializable]
public class ObjectData
{
	public string label;

	[Header("Set Up")]
	public float scale;
	public float gravity;
	public Sprite sprite;
	public Color color;
}
