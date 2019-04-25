using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class Trapezoid : MonoBehaviour
{
	public Color color;
	public bool isHighlighted;

	[SerializeField]
	Trapezoid topLeftTrap;
	public Trapezoid TopLeftTrap
	{
		get { return topLeftTrap; }
		set
		{
			if (value)
			{
				value.topRightTrap = this;
			}
			topLeftTrap = value;
		}
	}
	[SerializeField]
	Trapezoid topRightTrap;
	public Trapezoid TopRightTrap
	{
		get { return topRightTrap; }
		set
		{
			if (value)
			{
				value.topLeftTrap = this;
			}
			topRightTrap = value;
		}
	}
	[SerializeField]
	Trapezoid bottomLeftTrap;
	public Trapezoid BottomLeftTrap
	{
		get { return bottomLeftTrap; }
		set
		{
			if (value)
			{
				value.bottomRightTrap = this;
			}
			bottomLeftTrap = value;
		}
	}
	[SerializeField]
	Trapezoid bottomRightTrap;
	public Trapezoid BottomRightTrap
	{
		get { return bottomRightTrap; }
		set
		{
			if (value)
			{
				value.bottomLeftTrap = this;
			}
			bottomRightTrap = value;
		}
	}
	public Vertex LeftVertex { get; set; }
	public Vertex RightVertex { get; set; }
	public Edge TopEdge { get; set; }
	public Edge BottomEdge { get; set; }
	TrapezoidNode node;
	public TrapezoidNode Node
	{
		get { return node; }
		set
		{
			if (node != value)
			{
				node = value;
				if (value)
				{
					value.Data = this;
				}
			}
		}
	}
	
	static int trapezoidCount = 0;

	public int id;

	void Awake()
	{
		id = trapezoidCount;
		trapezoidCount++;
	}

	// Use this for initialization
	void Start()
	{
		Mesh mesh = new Mesh();

		float xLeft = LeftVertex.transform.position.x;
		float xRight = RightVertex.transform.position.x;

		// Create vertices
		Vector3[] vertices = new Vector3[4];
		vertices[0] = new Vector3(xLeft, BottomEdge.CalculateYCoord(xLeft), 1);
		vertices[1] = new Vector3(xRight, BottomEdge.CalculateYCoord(xRight), 1);
		vertices[2] = new Vector3(xLeft, TopEdge.CalculateYCoord(xLeft), 1);
		vertices[3] = new Vector3(xRight, TopEdge.CalculateYCoord(xRight), 1);
		mesh.vertices = vertices;

		// Create triangles
		int[] triangles = {0, 2, 1,   2, 3, 1};
		mesh.triangles = triangles;

		// Create normals
		Vector3[] normals = new Vector3[4];
		normals[0] = -Vector3.forward;
		normals[1] = -Vector3.forward;
		normals[2] = -Vector3.forward;
		normals[3] = -Vector3.forward;
		mesh.normals = normals;

		// Create UVs
		Vector2[] uv = new Vector2[4];
		uv[0] = new Vector2(0, 0);
		uv[1] = new Vector2(1, 0);
		uv[2] = new Vector2(0, 1);
		uv[3] = new Vector2(1, 1);
		mesh.uv = uv;

		// Assign collider
		GetComponent<MeshCollider>().sharedMesh = mesh;

		// Assign mesh
		GetComponent<MeshFilter>().mesh = mesh;

		// Create material
		GetComponent<MeshRenderer>().material = new Material(Shader.Find("Unlit/Transparent"));

		// Random color
		color = Random.ColorHSV(0, 1, 0.3f, 0.3f, 1, 1);
	}

	// Update is called once per frame
	void Update()
	{
		Color matColor = color;
		if (isHighlighted)
		{
			matColor = Color.red;
			matColor.a = 0.5f;
		}
		GetComponent<MeshRenderer>().material.color = matColor;
	}

	public Trapezoid FindRightTrapezoid(Edge edge)
	{
		float edgeY = edge.CalculateYCoord(RightVertex.transform.position.x);
		if (edgeY > RightVertex.transform.position.y)
		{
			return TopRightTrap;
		}
		else
		{
			return BottomRightTrap;
		}
	}
}
