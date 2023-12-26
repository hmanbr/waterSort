using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;

	// Update is called once per frame
	void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

            if(hit.collider != null )
            {
				Lane laneComponent = hit.collider.GetComponent<Lane>();
				
				if (laneComponent != null)
				{
					if(!gameManager.HasChosenLane())
					{
						gameManager.SetChosenLane(laneComponent);

					}else
					{
						gameManager.AttemptPour(laneComponent);
					}

				}
			}
        }
    }
}
