using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Lane : MonoBehaviour
{

	[SerializeField] private List<GameObject> allShapePosition = new List<GameObject>(4);
	[SerializeField] private ParticleSystem firework;

	public List<GameObject> GetAllShapesPosition()
	{
		return allShapePosition;
	}

	public void PlayVFX()
	{
		firework.Play();
	}

	public bool IsComplete()
	{
		List<Shape> allShapes = GetAllShapes();
		Color firstColor = allShapes[0].GetColor();
		int count = 0;
		foreach (Shape shape in allShapes)
		{
			if (shape.GetColor() == firstColor)
			{
				count++;
			}
		}
		return count == 4;
	}

	public int NumberOfShapes()
	{
		int count = 0;
		foreach (GameObject obj in allShapePosition)
		{
			if (HasChild(obj))
			{
				count++;
			}
			else
			{
				break;
			}
		}
		return count;
	}

	public int EmptyPositions()
	{
		int count = 0;
		for (int i = 3; i >= 0; i--)
		{
			if (!HasChild(allShapePosition[i]))
			{
				count++;

			}
			else
			{
				break;
			}

		}
		return count;
	}

	public bool LaneFillable(Shape shapeToCheck)
	{
		if (NumberOfShapes() == 0)
		{
			return true;
		}
		else
		{
			if (NumberOfShapes() == 4)
			{
				return false;
			}
			else
			{
				if (GetTopShape().GetColor() == shapeToCheck.GetColor())
				{
					return true;
				}
				else
				{
					return false;
				}
			}
		}
	}

	public int NumberOfShapeToPour(int emptySlot, Shape shapeToPour)
	{
		if (shapeToPour == null)
		{
			shapeToPour = GetTopShape();
		}

		List<Shape> shapeList = GetAllShapes();
		int count = 0;
		for (int i = 1; i <= emptySlot; i++)
		{
			if (i > shapeList.Count)
			{
				break;
			}

			if (shapeList[shapeList.Count - i].GetColor() == shapeToPour.GetColor())
			{
				count++;
			}
			else
			{
				break;
			}
		}
		return count;
	}

	public List<Shape> GetAllShapes()
	{
		List<Shape> shapeList = new List<Shape>();
		foreach (GameObject obj in allShapePosition)
		{
			if (HasChild(obj))
			{
				shapeList.Add(obj.transform.GetComponentInChildren<Shape>());
			}
		}
		return shapeList;
	}

	public Shape GetTopShape()
	{
		List<Shape> shapeList = GetAllShapes();
		if (shapeList.Count == 0)
		{
			return null;
		}
		return shapeList[shapeList.Count - 1];
	}

	public int FindFirstEmptyIndex()
	{
		for (int i = 0; i <= 3; i++)
		{
			if (!HasChild(allShapePosition[i]))
			{
				return i;
			}
		}
		return -1;
	}

	public int FindLastIndexWithChild()
	{
		for (int i = 3; i >= 0; i--)
		{
			if (HasChild(allShapePosition[i]))
			{
				return i;
			}
		}
		return -1;
	}

	public bool HasChild(GameObject obj)
	{
		return obj.transform.childCount > 0;
	}
}
