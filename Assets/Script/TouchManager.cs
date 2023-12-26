using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchManager : MonoBehaviour
{
	[SerializeField] private GameManager gameManager;
	private TouchInput touchInput;

	private void Awake()
	{
		touchInput = new TouchInput();	
		touchInput.Touch.Enable();
		touchInput.Touch.TouchPress.performed += TouchPress_performed;
	}

	private void TouchPress_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
	{
		DoStuff();
	}

	public void DoStuff()
	{
		/*Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);*/
		Vector2 mousePos2D = Camera.main.ScreenToWorldPoint(touchInput.Touch.TouchPosition.ReadValue<Vector2>());

		RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

		if (hit.collider != null)
		{
			Lane laneComponent = hit.collider.GetComponent<Lane>();

			if (laneComponent != null)
			{
				if (!gameManager.HasChosenLane())
				{
					gameManager.SetChosenLane(laneComponent);

				}
				else
				{
					gameManager.AttemptPour(laneComponent);
				}

			}
		}
	}

	// Update is called once per frame
	void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			
		}
	}
}
