using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameMenu : MonoBehaviour
{

	[SerializeField] private GameObject nextLevelButton;
	public void NextLevelButtonDeactivate()
	{
		nextLevelButton.SetActive(false);
	}

	public void NextLevelButtonActivate()
	{
		nextLevelButton.SetActive(true);
	}

	public void QuitGame()
	{
		Application.Quit();
	}
}
