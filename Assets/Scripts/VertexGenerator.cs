using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VertexGenerator : MonoBehaviour
{
	public GameObject vertexPrefab;

	public HashSet<Vertex> Vertices { get; private set; }

	// Use this for initialization
	void Start()
	{
		Vertices = new HashSet<Vertex>();
	}

	// Update is called once per frame
	void Update()
	{
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		Vector3 worldPos = new Vector3(ray.origin.x, ray.origin.y, 0);
		
		// Left click adds point
		if (Input.GetMouseButtonDown(0))
		{
			bool doAdd = true;
			RaycastHit[] hits = Physics.RaycastAll(ray);
			foreach (var hit in hits)
			{
				if (hit.transform.GetComponent<Vertex>() != null)
				{
					doAdd = false;
					break;
				}
			}
			if (doAdd)
			{
				Vertex vert = Instantiate(vertexPrefab, worldPos, Quaternion.identity).GetComponent<Vertex>();
				Vertices.Add(vert);
			}
		}
		// Right click removes point
		else if (Input.GetMouseButtonDown(1))
		{
			RaycastHit[] hits = Physics.RaycastAll(ray);
			foreach (var hit in hits)
			{
				if (hit.transform.GetComponent<Vertex>() != null)
				{
					Vertex vert = hit.transform.GetComponent<Vertex>();
					Vertices.Remove(vert);
					Destroy(hit.transform.gameObject);
					break;
				}
			}
		}
	}
}
