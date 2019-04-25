using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(BoxCollider))]
public class Edge : MonoBehaviour
{
	public Material defaultMaterial;
	public Material fadedMaterial;
	public Material highlightMaterial;
	public bool isHighlighted;
	public bool isFaded;

	Vertex point1;
	public Vertex Point1
	{
		get { return point1; }
		set
		{
			if (value)
			{
				value.Degree++;
			}
			if (point1)
			{
				point1.Degree--;
			}
			point1 = value;
		}
	}
	Vertex point2;
	public Vertex Point2
	{
		get { return point2; }
		set
		{
			if (value)
			{
				value.Degree++;
			}
			if (point2)
			{
				point2.Degree--;
			}
			point2 = value;
		}
	}
	public Vertex LeftVertex
	{
		get
		{
			if (Point1.transform.position.x < Point2.transform.position.x)
			{
				return Point1;
			}
			else
			{
				return Point2;
			}
		}
	}
	public Vertex RightVertex
	{
		get
		{
			if (Point1.transform.position.x > Point2.transform.position.x)
			{
				return Point1;
			}
			else
			{
				return Point2;
			}
		}
	}

	static int edgeCount = 0;

	LineRenderer lineRenderer;
	BoxCollider boxCollider;
	int id;

	void Awake()
	{
		id = edgeCount;
		edgeCount++;
		lineRenderer = GetComponent<LineRenderer>();
		lineRenderer.positionCount = 2;
		boxCollider = GetComponent<BoxCollider>();
	}

	// Use this for initialization
	void Start()
	{
	}

	// Update is called once per frame
	void Update()
	{
		// Get point positions
		Vector2 pos1 = Vector2.zero;
		if (Point1)
		{
			pos1 = Point1.transform.position;
		}
		Vector2 pos2 = Vector2.zero;
		if (Point2)
		{
			pos2 = Point2.transform.position;
		}

		SetPosition(pos1, pos2);
		
		// Highlight line
		if (isHighlighted)
		{
			lineRenderer.material = highlightMaterial;
		}
		else if (isFaded)
		{
			lineRenderer.material = fadedMaterial;
		}
		else
		{
			lineRenderer.material = defaultMaterial;
		}
	}

	public void SetPosition(Vector2 pos1, Vector2 pos2)
	{
		// Set line position
		lineRenderer.SetPosition(0, pos1);
		lineRenderer.SetPosition(1, pos2);

		// Adjust box collider
		boxCollider.size = new Vector3(Vector3.Distance(pos1, pos2), lineRenderer.startWidth, lineRenderer.startWidth);
		transform.position = (pos1+pos2) / 2;
		transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(pos2.y-pos1.y, pos2.x-pos1.x) * Mathf.Rad2Deg);
	}

	public float CalculateYCoord(float px)
	{
		float t = (px - Point1.transform.position.x) / (Point2.transform.position.x - Point1.transform.position.x);
		return Point1.transform.position.y * (1 - t) + Point2.transform.position.y * t;
	}

	public override int GetHashCode()
	{
		return id;
	}

	public override bool Equals(object obj)
	{
		if (obj is Edge)
		{
			Edge other = obj as Edge;
			return id == other.id;
		}
		return false;
	}
}
