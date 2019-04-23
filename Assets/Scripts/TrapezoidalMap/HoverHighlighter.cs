using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverHighlighter : MonoBehaviour
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

		Ray mouseRay = dagCamera.ScreenPointToRay(Input.mousePosition);
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

		foreach (Node last in lastTouch)
		{
			SetHighlight(last, false);
		}
		foreach (Node curr in currTouch)
		{
			SetHighlight(curr, true);
		}

		lastTouch.Clear();
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
