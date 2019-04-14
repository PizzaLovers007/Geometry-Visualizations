using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vertex : MonoBehaviour
{
	public Material defaultMaterial;
	public Material highlightMaterial;
	public bool isHighlighted;

	public int Degree { get; set; }

	static int vertexCount = 0;

	new Renderer renderer;
	int id;

	void Awake()
	{
		id = vertexCount;
		vertexCount++;
	}

	// Use this for initialization
	void Start()
	{
		renderer = GetComponent<Renderer>();
	}

	// Update is called once per frame
	void Update()
	{
		if (isHighlighted)
		{
			renderer.material = highlightMaterial;
		}
		else
		{
			renderer.material = defaultMaterial;
		}
	}

	public override int GetHashCode()
	{
		return id;
	}

	public override bool Equals(object obj)
	{
		if (obj is Vertex)
		{
			Vertex other = obj as Vertex;
			return id == other.id;
		}
		return false;
	}
}
