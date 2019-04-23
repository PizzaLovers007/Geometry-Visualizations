using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverTrapToDag : MonoBehaviour
{
	TrapezoidalMap trapezoidalMap;
	Vector3 lastPos;
	Trapezoid lastTrap;

	// Use this for initialization
	void Start()
	{
		trapezoidalMap = GameObject.Find("TrapezoidalMap").GetComponent<TrapezoidalMap>();
	}

	// Update is called once per frame
	void Update()
	{
		if (trapezoidalMap.IsGenerating)
		{
			if (lastTrap)
			{
				trapezoidalMap.HighlightPath(lastPos, false);
				lastTrap = null;
			}
			return;
		}

		trapezoidalMap.HighlightPath(lastPos, false);

		Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		Vector3 viewPoint = Camera.main.WorldToViewportPoint(mouseRay.origin);
		if (viewPoint.x < 0 || viewPoint.x > 1 || viewPoint.y < 0 || viewPoint.y > 1)
		{
			return;
		}
		Vector3 worldPos = new Vector3(mouseRay.origin.x, mouseRay.origin.y, 0);

		Trapezoid newTrap = trapezoidalMap.LocatePoint(worldPos);
		
		trapezoidalMap.HighlightPath(worldPos, true);

		lastPos = worldPos;
		lastTrap = newTrap;
	}
}
