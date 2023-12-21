using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shape : MonoBehaviour
{
	[SerializeField] private int shapeId;
	private Color color;

	private void Start()
	{
		SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
		color = spriteRenderer.color;
	}

	public int GetShapeId() { return shapeId; }

	public Color GetColor()
	{
		return color;
	}
}
