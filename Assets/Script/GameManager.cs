using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	public event EventHandler OnNoMoreLevel;
	public event EventHandler OnNoMoreMoves;

	[SerializeField] Lane lanePrefab;
	[SerializeField] TextMeshProUGUI levelText;
	[SerializeField] List<Transform> LaneSpawnPositions = new List<Transform>();
	[SerializeField] private List<Shape> allShapes = new List<Shape>();

	private Lane chosenLane;
	private Lane receiverLane;
	float chosenOffset = 0.75f;

	private int numberOfLevels;
	[SerializeField] private int currentLevel = 1;

	private int numberOfLane;
	private int numberOfCompletedLane = 0;
	private List<Lane> allLanes = new List<Lane>();

	// Start is called before the first frame update
	void Start()
	{
		InitializeLevel();
	}

	public void SetChosenLane(Lane lane)
	{
		if (chosenLane == null)
		{
			chosenLane = lane;

			PopLaneUp(chosenLane);
		}
	}

	public void AttemptPour(Lane lane)
	{
		receiverLane = lane;
		Shape chosenShape = chosenLane.GetTopShape();

/*		Debug.Log("Attemp Pour " + chosenLane + " to " + receiverLane);
		Debug.Log("Fillable status " + (receiverLane.LaneFillable(chosenShape)));*/


		if (chosenLane.NumberOfShapes() == 0 || chosenLane.IsComplete()) //might need to give Lane an isComplete bool to avoid calling IsComplete()
		{
			PopLaneDown(chosenLane);
			chosenLane = null;
			receiverLane = null;
		}
		else if (receiverLane.LaneFillable(chosenShape)) //check if reciever is fillable
		{
			if (receiverLane == chosenLane)
			{
				PopLaneDown(chosenLane);
				receiverLane = null;
				chosenLane = null;
			}
			else //pour attemp
			{
				List<GameObject> chosenLaneAllShapePos = chosenLane.GetAllShapesPosition();
				List<GameObject> reciverLaneAllShapePos = receiverLane.GetAllShapesPosition();

				int numOfEmpty = receiverLane.EmptyPositions();
				int numOfShapeToPour = chosenLane.NumberOfShapeToPour(numOfEmpty, receiverLane.GetTopShape());
				Debug.Log("num of shape to pour " + numOfShapeToPour);
				int reciverFirstEmptyIndex = receiverLane.FindFirstEmptyIndex();
				int chosenLastIndexWithChild = chosenLane.FindLastIndexWithChild();

				for (int i = 1; i <= numOfShapeToPour; i++)
				{
					Transform shapeChild = chosenLaneAllShapePos[chosenLastIndexWithChild].transform.GetChild(0);
					shapeChild.parent = reciverLaneAllShapePos[reciverFirstEmptyIndex].transform;
					shapeChild.localPosition = UnityEngine.Vector3.zero;
					chosenLastIndexWithChild--;
					reciverFirstEmptyIndex++;
				}
				PopLaneDown(chosenLane);

				if(receiverLane.IsComplete())
				{
					numberOfCompletedLane++;
					receiverLane.PlayVFX();
					WinCondition();
				}
				receiverLane = null;
				chosenLane = null;

			}

		}
		else
		{
			Debug.Log("chosen an unfillable lane");
			PopLaneDown(chosenLane);
			chosenLane = receiverLane;
			PopLaneUp(chosenLane);
			receiverLane = null;
		}
		LoseCondition();
	}

	private void PopLaneDown(Lane lane)
	{
		UnityEngine.Vector3 newPosition = lane.transform.position;
		newPosition.y -= chosenOffset;
		lane.transform.position = newPosition;
	}

	private void PopLaneUp(Lane lane)
	{
		UnityEngine.Vector3 newPosition = lane.transform.position;
		newPosition.y += chosenOffset;
		lane.transform.position = newPosition;
	}

	public bool ShapeCoditionMatched(Shape chosenShape, Shape receiverShape)
	{
		if (chosenShape != null && receiverShape == null)
		{
			return true;
		}
		else if (chosenShape.GetColor() != receiverShape.GetColor())
		{
			return false;
		}
		else
		{
			return true;
		}
	}
	public bool HasChosenLane()
	{
		return chosenLane != null;
	}

	public void InitializeLevel()
	{
		levelText.text = "Level " + currentLevel; //bad gui location
		TextAsset jsonFile = Resources.Load<TextAsset>("LevelData");

		if (jsonFile != null)
		{
			LevelContainer levelContainer = JsonUtility.FromJson<LevelContainer>(jsonFile.text);


			numberOfLevels = levelContainer.Level.Count;
			if (currentLevel <= numberOfLevels)
			{
				numberOfLane = levelContainer.Level[currentLevel - 1].numLane;
				List<int> shapeIdList = levelContainer.Level[currentLevel - 1].ShapeColorType;
				int currentShapeIndex = 0;

				if (allLanes.Count > 0)
				{
					foreach (Lane lane in allLanes)
					{
						GameObject.Destroy(lane.gameObject);
					}
				}

				allLanes.Clear();

				int newlanecount = 1; //debug purpose
				for (int i = 1; i <= numberOfLane; i++)
				{

					Lane newLane = GameObject.Instantiate(lanePrefab);
					allLanes.Add(newLane);
					newLane.transform.position = LaneSpawnPositions[i - 1].position;
					newLane.name += newlanecount; //debug purpose
					newlanecount++; //debug purpose

					List<GameObject> allShapePos = newLane.GetAllShapesPosition();

					if (i <= numberOfLane - 2)
					{
						foreach (GameObject go in allShapePos)
						{
							int newShapeId = shapeIdList[currentShapeIndex];
							Shape newShape = GameObject.Instantiate(allShapes[newShapeId - 1], go.transform);

							newShape.transform.localPosition = UnityEngine.Vector3.zero;
							currentShapeIndex++;
						}
					}
				}
				
			}
			else
			{
				Debug.Log("We got no more levels");
				OnNoMoreLevel?.Invoke(this, EventArgs.Empty);
			}

		}
		else
		{
			Debug.LogError("Failed to load JSON file");
		}
	}

	public void WinCondition()
	{
		if(numberOfCompletedLane == numberOfLane - 2)
		{
			currentLevel++;
			numberOfCompletedLane = 0;
			InitializeLevel();
		}
	}

	public void LoseCondition()
	{
		//get all top shape, put in a list
		//check if nothing in list is dupe(all shape is distict) => lose
		//if shape dupe, check if the all lanes that contain any dupe have any more slot above it => if no slot, lose
		List<int> allTopShapesId = new List<int>();
		foreach(Lane lane in allLanes)
		{
			Shape topShape = lane.GetTopShape();
			if(topShape == null)
			{
				return;
			}else
			{
				allTopShapesId.Add(topShape.GetShapeId());
			}
		}
		bool areAllDistinct = allTopShapesId.Distinct().Count() == allTopShapesId.Count;

		if(areAllDistinct)
		{
			Debug.Log("Lose invoke");
			OnNoMoreMoves?.Invoke(this, EventArgs.Empty);
		}else
		{
			List<int> indicesWithDupe = FindDuplicateIndices(allTopShapesId);
			/*foreach(int shapeId in indicesWithDupe) { Debug.Log("lane" + shapeId); } //debug*/

			bool winnable = false;
			foreach(int index in indicesWithDupe)
			{
				
				if (allLanes[index].NumberOfShapes() < 4) {
					winnable = true; break;
				}
			}
			if(!winnable)
			{
				Debug.Log("Lose condition invoke");
				OnNoMoreMoves?.Invoke(this, EventArgs.Empty);
			}
		}

	}

	List<int> FindDuplicateIndices(List<int> data)
	{
		List<int> duplicates = data
	.Select((num, index) => new { Index = index, Number = num })
	.GroupBy(item => item.Number)
	.Where(group => group.Count() > 1)
	.SelectMany(group => group.Select(item => item.Index))
	.ToList();
		return duplicates;
	}

	[System.Serializable]
	public class Level
	{
		public int numLane;
		public List<int> ShapeColorType;
	}

	[System.Serializable]
	public class LevelContainer
	{
		public List<Level> Level;
	}

}


