using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EdgeGenerator : MonoBehaviour
{
	public GameObject vertexPrefab;
	public GameObject edgePrefab;
	public float dragThreshold = 0.1f;

	public HashSet<Vertex> Vertices { get; private set; }
	public HashSet<Edge> Edges { get; private set; }

	bool isPlacing;
	float delta;
	Vector3 lastPosition;
	Edge currEdge;
	TrapezoidalMap trapezoidalMap;

	// Use this for initialization
	void Start()
	{
		Vertices = new HashSet<Vertex>();
		Edges = new HashSet<Edge>();
		trapezoidalMap = GameObject.Find("TrapezoidalMap").GetComponent<TrapezoidalMap>();
	}

	// Update is called once per frame
	void Update()
	{
		if (trapezoidalMap.IsGenerating)
		{
			return;
		}

		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		Vector3 viewPoint = Camera.main.WorldToViewportPoint(ray.origin);
		if (viewPoint.x < 0 || viewPoint.x > 1 || viewPoint.y < 0 || viewPoint.y > 1)
		{
			return;
		}
		Vector3 worldPos = new Vector3(ray.origin.x, ray.origin.y, 0);

		if (!isPlacing)
		{
			if (Input.GetMouseButtonDown(0))
			{
				// Set state info
				lastPosition = worldPos;
				delta = 0;
				isPlacing = true;

				// Find existing vertex
				Vertex startVert = null;
				RaycastHit[] hits = Physics.RaycastAll(ray);
				foreach (var hit in hits)
				{
					if (hit.transform.GetComponent<Vertex>() != null)
					{
						startVert = hit.transform.GetComponent<Vertex>();
						break;
					}
				}

				// Create if no existing vertex found
				if (!startVert)
				{
					startVert = Instantiate(vertexPrefab, worldPos, Quaternion.identity).GetComponent<Vertex>();
					Vertices.Add(startVert);
				}
			
				// Create end vertex
				Vertex endVert = Instantiate(vertexPrefab, worldPos, Quaternion.identity).GetComponent<Vertex>();
				endVert.transform.position = startVert.transform.position;
				Vertices.Add(endVert);

				// Create edge and attach start/end vertices
				currEdge = Instantiate(edgePrefab).GetComponent<Edge>();
				currEdge.Point1 = startVert;
				currEdge.Point2 = endVert;
				Edges.Add(currEdge);
			}
			else if (Input.GetMouseButtonDown(1))
			{
				// Find edge/vertex clicked on
				RaycastHit[] hits = Physics.RaycastAll(ray);
				Vertex vert = null;
				Edge edge = null;
				foreach (var hit in hits)
				{
					if (hit.transform.GetComponent<Vertex>() != null)
					{
						vert = hit.transform.GetComponent<Vertex>();
						break;
					}
					if (hit.transform.GetComponent<Edge>() != null)
					{
						edge = hit.transform.GetComponent<Edge>();
						break;
					}
				}

				if (vert)
				{
					// Find connected edges
					HashSet<Edge> toRemove = new HashSet<Edge>();
					foreach (Edge e in Edges)
					{
						if (e.Point1 == vert || e.Point2 == vert)
						{
							toRemove.Add(e);
						}
					}

					// Delete connected edges
					foreach (Edge e in toRemove)
					{
						// Delete connected vertices if this is the only edge
						e.Point1.Degree--;
						e.Point2.Degree--;
						if (e.Point1.Degree == 0)
						{
							Vertices.Remove(e.Point1);
							Destroy(e.Point1.gameObject);
						}
						if (e.Point2.Degree == 0)
						{
							Vertices.Remove(e.Point2);
							Destroy(e.Point2.gameObject);
						}
						Edges.Remove(e);
						Destroy(e.gameObject);
					}

					// Delete vertex clicked on
					Vertices.Remove(vert);
					Destroy(vert.gameObject);
				}
				else if (edge)
				{
					// Delete connected vertices if this is the only edge
					edge.Point1.Degree--;
					edge.Point2.Degree--;
					if (edge.Point1.Degree == 0)
					{
						Vertices.Remove(edge.Point1);
						Destroy(edge.Point1.gameObject);
					}
					if (edge.Point2.Degree == 0)
					{
						Vertices.Remove(edge.Point2);
						Destroy(edge.Point2.gameObject);
					}

					// Delete edge clicked on
					Edges.Remove(edge);
					Destroy(edge.gameObject);
				}
			}
		}
		else
		{
			// Update mouse delta
			delta += Vector3.Distance(lastPosition, worldPos);
			lastPosition = worldPos;

			// If mouse was just clicked
			if (Input.GetMouseButtonDown(0))
			{
				// Finish placing
				currEdge.Point2.transform.position = worldPos;
				isPlacing = false;
			}
			// If mouse was just released
			else if (Input.GetMouseButtonUp(0))
			{
				// Finish placing if mouse was dragged past threshold
				if (delta > dragThreshold)
				{
					currEdge.Point2.transform.position = worldPos;
					isPlacing = false;
				}
			}
			// If mouse is currently being pressed
			else if (Input.GetMouseButton(0))
			{
				// Update point position if threshold has been passed
				if (delta > dragThreshold)
				{
					currEdge.Point2.transform.position = worldPos;
				}
			}
		}
	}
}
