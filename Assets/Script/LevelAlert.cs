using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LevelAlert : MonoBehaviour
{
	[SerializeField] TextMeshProUGUI levelAlertText;
	[SerializeField] GameManager gameManager;

	private void Start()
	{
		gameManager.OnNoMoreLevel += GameManager_OnNoMoreLevel;
	}

	private void GameManager_OnNoMoreLevel(object sender, System.EventArgs e)
	{
		Debug.Log("Listend");
		levelAlertText.enabled = true;
	}
}
