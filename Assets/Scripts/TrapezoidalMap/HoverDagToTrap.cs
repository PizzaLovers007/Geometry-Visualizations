using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverDagToTrap : MonoBehaviour
{
	TrapezoidalMap trapezoidalMap;
	Camera dagCamera;
	HashSet<Node> lastTouch;
	HashSet<Node> currTouch;

	void Awake()
	{
		lastTouch = new HashSet<Node>();
		currTouch = new HashSet<Node>();
	}

	// Use this for initialization
	void Start()
	{
		trapezoidalMap = GameObject.Find("TrapezoidalMap").GetComponent<TrapezoidalMap>();
		dagCamera = GameObject.Find("DAG Camera").GetComponent<Camera>();
	}

	// Update is called once per frame
	void Update()
	{
		if (trapezoidalMap.IsGenerating)
		{
			foreach (Node last in lastTouch)
			{
				SetHighlight(last, false);
			}
			lastTouch.Clear();
			return;
		}
		
		foreach (Node last in lastTouch)
		{
			SetHighlight(last, false);
		}
		lastTouch.Clear();

		Ray mouseRay = dagCamera.ScreenPointToRay(Input.mousePosition);
		Vector3 viewPoint = dagCamera.WorldToViewportPoint(mouseRay.origin);
		if (viewPoint.x < 0 || viewPoint.x > 1 || viewPoint.y < 0 || viewPoint.y > 1)
		{
			return;
		}
		RaycastHit[] hits = Physics.RaycastAll(mouseRay);
		currTouch.Clear();
		foreach (RaycastHit hit in hits)
		{
			Node node = hit.collider.GetComponent<Node>();
			if (node)
			{
				currTouch.Add(node);
			}
		}

		foreach (Node curr in currTouch)
		{
			SetHighlight(curr, true);
		}

		lastTouch.UnionWith(currTouch);
	}

	void SetHighlight(Node node, bool highlight)
	{
		if (node is TrapezoidNode)
		{
			(node as TrapezoidNode).Data.isHighlighted = highlight;
		}
		else if (node is EdgeNode)
		{
			(node as EdgeNode).Data.isHighlighted = highlight;
		}
		else if (node is VertexNode)
		{
			(node as VertexNode).Data.isHighlighted = highlight;
		}
	}
}
