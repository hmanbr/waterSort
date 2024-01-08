using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static GameManager;

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

	private int numberOfLevelsInList;
	private int currentLevel = 1; //probally delete this cus its saved in json

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
/*				Debug.Log("num of shape to pour " + numOfShapeToPour);
*/				int reciverFirstEmptyIndex = receiverLane.FindFirstEmptyIndex();
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

				if (receiverLane.IsComplete())
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
/*			Debug.Log("chosen an unfillable lane");*/
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


	public bool HasChosenLane()
	{
		return chosenLane != null;
	}

	public void InitializeLevel()
	{
		//Convoluted af
		//If current level is within list of pre-made level -> spawn level
		//If current level is outside list of pre-made level, check if the generated level in json is beaten -> if not countinue play
		//If generated level in json is beaten -> generate new level, save that level to current level
#if UNITY_EDITOR

		UnityEditor.AssetDatabase.Refresh();

#endif

		TextAsset levelListJson = Resources.Load<TextAsset>("LevelData");

		if (levelListJson != null)
		{
			LevelContainer levelContainer = JsonUtility.FromJson<LevelContainer>(levelListJson.text);
			currentLevel = levelContainer.currentLevel;

			levelText.text = "Level " + currentLevel; //temp, bad gui location

			numberOfLevelsInList = levelContainer.Levels.Count;
			
			if (currentLevel <= numberOfLevelsInList)
			{
				numberOfLane = levelContainer.Levels[currentLevel - 1].numLane;
				List<int> shapeIdList = levelContainer.Levels[currentLevel - 1].ShapeColorType;
				Debug.Log("we here now " + levelContainer.currentLevel);
				SpawnLevelLanesAndShapes(numberOfLane, shapeIdList);

			}
			else
			{
				TextAsset generatedLevelJson = Resources.Load<TextAsset>("GeneratedLevel");
				if (generatedLevelJson != null && !string.IsNullOrEmpty(generatedLevelJson.text))
				{
					GeneratedLevelData generatedLevelData = JsonUtility.FromJson<GeneratedLevelData>(generatedLevelJson.text);

					if (generatedLevelData.levelNumber < levelContainer.currentLevel)
					{
						Level newLevel = GenerateLevel();
						GeneratedLevelData newLevelData = new GeneratedLevelData()
						{
							levelNumber = ++generatedLevelData.levelNumber,
							Level = newLevel
						};

						numberOfLane = newLevel.numLane;
						List<int> shapeIdList = newLevel.ShapeColorType;
						SpawnLevelLanesAndShapes(numberOfLane, shapeIdList);

						string newLevelJson = JsonUtility.ToJson(newLevelData);
						File.WriteAllText(Application.dataPath + "/Resources/GeneratedLevel.json", newLevelJson);

						levelContainer.currentLevel++;
						string updateLevelListJson = JsonUtility.ToJson(levelContainer);
						File.WriteAllText(Application.dataPath + "/Resources/LevelData.json", updateLevelListJson);
					}
					else if (generatedLevelData.levelNumber == levelContainer.currentLevel)
					{
						SpawnLevelLanesAndShapes(numberOfLane, generatedLevelData.Level.ShapeColorType);
					}
				}else
				{
					Debug.Log("null shitz");
					Level newLevel = GenerateLevel();
					GeneratedLevelData newLevelData = new GeneratedLevelData()
					{
						levelNumber = currentLevel + 1,
						Level = newLevel
					};

					numberOfLane = newLevel.numLane;
					List<int> shapeIdList = newLevel.ShapeColorType;
					SpawnLevelLanesAndShapes(numberOfLane, shapeIdList);

					string newLevelJson = JsonUtility.ToJson(newLevelData);
					File.WriteAllText(Application.dataPath + "/Resources/GeneratedLevel.json", newLevelJson);

					levelContainer.currentLevel++;
					string updateLevelListJson = JsonUtility.ToJson(levelContainer);
					File.WriteAllText(Application.dataPath + "/Resources/LevelData.json", updateLevelListJson);
				}
				Resources.UnloadAsset(generatedLevelJson);
			}

		}
		else
		{
			Debug.LogError("Failed to load JSON file");
		}
		Resources.UnloadAsset(levelListJson);
		
	}

	public void SpawnLevelLanesAndShapes(int levelNumberOfLane, List<int> shapeIdList)
	{
		Debug.Log("we there now");
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
		for (int i = 1; i <= levelNumberOfLane; i++)
		{

			Lane newLane = GameObject.Instantiate(lanePrefab);
			allLanes.Add(newLane);
			newLane.transform.position = LaneSpawnPositions[i - 1].position;
			newLane.name += newlanecount; //debug purpose
			newlanecount++; //debug purpose

			List<GameObject> allShapePos = newLane.GetAllShapesPosition();

			if (i <= levelNumberOfLane - 2)
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

	public void WinCondition()
	{
		if (numberOfCompletedLane == numberOfLane - 2)
		{
			numberOfCompletedLane = 0;

			//save current level in file
			TextAsset levelListJson = Resources.Load<TextAsset>("LevelData");
			if(levelListJson != null)
			{
				LevelContainer levelContainer = JsonUtility.FromJson<LevelContainer>(levelListJson.text);
				levelContainer.currentLevel++;
				string updateLevelListJson = JsonUtility.ToJson(levelContainer);
				File.WriteAllText(Application.dataPath + "/Resources/LevelData.json", updateLevelListJson);
				Resources.UnloadAsset(levelListJson);
			}
			

			InitializeLevel();
		}
	}

	public void LoseCondition() // missing case where top slot for dupe is avalible, but still stuck
	{
		//get all top shape, put in a list
		//check if nothing in list is dupe(all shape is distict) => lose
		//if shape dupe, check if the all lanes that contain any dupe have any more slot above it => if no slot, lose
		List<int> allTopShapesId = new List<int>();
		foreach (Lane lane in allLanes)
		{
			Shape topShape = lane.GetTopShape();
			if (topShape == null)
			{
				return;
			}
			else
			{
				allTopShapesId.Add(topShape.GetShapeId());
			}
		}
		bool areAllDistinct = allTopShapesId.Distinct().Count() == allTopShapesId.Count;

		if (areAllDistinct)
		{
			Debug.Log("Lose invoke");
			OnNoMoreMoves?.Invoke(this, EventArgs.Empty);
		}
		else
		{
			List<int> indicesWithDupe = FindDuplicateIndices(allTopShapesId);
			/*foreach(int shapeId in indicesWithDupe) { Debug.Log("lane" + shapeId); } //debug*/

			bool winnable = false;
			foreach (int index in indicesWithDupe)
			{

				if (allLanes[index].NumberOfShapes() < 4)
				{
					winnable = true; break;
				}
			}
			if (!winnable)
			{
				Debug.Log("Lose condition invoke");
				OnNoMoreMoves?.Invoke(this, EventArgs.Empty);
			}
		}

	}

	public void ExtraLanePowerUp()
	{
		if (numberOfLane == 10)
		{
			return;
		}
		Lane newLane = GameObject.Instantiate(lanePrefab);
		allLanes.Add(newLane);
		newLane.transform.position = LaneSpawnPositions[numberOfLane].position;


		numberOfLane++;
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

	[System.Serializable]
	public class Level
	{
		public int numLane;
		public List<int> ShapeColorType;
	}

	[System.Serializable]
	public class LevelContainer
	{
		public int currentLevel;
		public List<Level> Levels;
	}

	[System.Serializable]
	public class GeneratedLevelData
	{
		public int levelNumber;
		public Level Level;
	}

	public Level GenerateLevel()
	{
		List<int> allShapesId = new List<int>();
		for (int i = 1; i < allShapes.Count; i++)
		{
			allShapesId.Add(i);
		}

		int numberofShapes = UnityEngine.Random.Range(2, 8);

		List<int> ChosenShapesId = new List<int>();
		for (int i = 0; i < numberofShapes; i++)
		{
			int index = UnityEngine.Random.Range(0, allShapesId.Count);
			int randomValue = allShapesId[index];

			// Add the selected random number to the result
			ChosenShapesId.Add(randomValue);

			// Remove the selected number from the available numbers to ensure uniqueness
			allShapesId.RemoveAt(index);
		}

		List<int> ShapeColorTypeResult = new List<int>();
		foreach (int shapeId in ChosenShapesId)
		{
			for (int i = 1; i <= 4; i++)
			{
				ShapeColorTypeResult.Add(shapeId);
			}
		}

		Shuffle(ShapeColorTypeResult);
		Level newGeneratedLevel = new Level()
		{
			numLane = numberofShapes + 2,
			ShapeColorType = ShapeColorTypeResult
		};
		return newGeneratedLevel;
	}

	private void Shuffle(List<int> list)
	{
		System.Random random = new System.Random();
		int n = list.Count;

		for (int i = n - 1; i > 0; i--)
		{
			int j = random.Next(0, i + 1);

			// Swap list[i] and list[j]
			int temp = list[i];
			list[i] = list[j];
			list[j] = temp;
		}
	}
}


