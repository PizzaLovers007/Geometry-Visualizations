using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JarvisMarch : MonoBehaviour
{
    public float speed = 0.1f;
    public GameObject edgePrefab;

    public bool IsGenerating { get; private set; }

	List<Vertex> hull { get; set; }
	List<Edge> edges { get; set; }

	VertexGenerator generator;
	PointCloud pointCloud;

	void Awake()
	{
		hull = new List<Vertex>();
		edges = new List<Edge>();
	}

	// Use this for initialization
	void Start()
	{
		generator = GameObject.Find("VertexGenerator").GetComponent<VertexGenerator>();
		pointCloud = GameObject.Find("PointCloud").GetComponent<PointCloud>();
	}

	// Update is called once per frame
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.D) && !IsGenerating)
        {
            foreach (Edge e in edges)
            {
                e.gameObject.SetActive(true);
                Destroy(e.gameObject);
            }
            edges.Clear();
            hull.Clear();
            generator.Clear();
        }

        if (Input.GetKeyDown(KeyCode.R) && !IsGenerating)
        {
            pointCloud.GenerateCloud();
        }

        if (Input.GetKeyDown(KeyCode.G) && !IsGenerating)
        {
            Debug.Log("Generating convex hull with Jarvis March");
            foreach (Edge e in edges)
            {
                e.gameObject.SetActive(true);
                Destroy(e.gameObject);
            }
            edges.Clear();
            hull.Clear();

            StartCoroutine(DoJarvisMarch(new List<Vertex>(generator.Vertices)));
        }
	}

	IEnumerator DoJarvisMarch(List<Vertex> vertices)
	{
		if (vertices.Count == 0)
		{
			yield break;
		}

		IsGenerating = true;

		// Find lowest vertex
		Vertex minVertex = vertices[0];
		foreach (Vertex v in vertices)
		{
			if (v.transform.position.y < minVertex.transform.position.y)
			{
				minVertex = v;
			}
		}

		// Setup curr/prev pointers
		Vector3 prev = new Vector2(-10000, minVertex.transform.position.y);
		Vertex prevVertex = null;
		Vertex currVertex = minVertex;

		// Faded edge to next best on hull
		Edge bestEdge = Instantiate(edgePrefab).GetComponent<Edge>();
		bestEdge.isFaded = true;

		while (currVertex != minVertex || prevVertex == null)
		{
			// Setup moving edge
			Edge currEdge = Instantiate(edgePrefab).GetComponent<Edge>();
			edges.Add(currEdge);
			currEdge.Point1 = currVertex;
			currEdge.Point2 = currVertex;
			currEdge.isHighlighted = true;
			bestEdge.Point1 = currVertex;
			bestEdge.Point2 = currVertex;
			bestEdge.SetPosition(currVertex.transform.position, currVertex.transform.position);

			// Find best angle vertex
			Vertex best = null;	
			foreach (Vertex nextVertex in vertices)
			{
				// Ignore current and previous vertex
				if (nextVertex == prevVertex || nextVertex == currVertex)
				{
					continue;
				}

				// Move edge
				currEdge.Point2 = nextVertex;
				currEdge.SetPosition(currVertex.transform.position, nextVertex.transform.position);

				if (best == null)
				{
					// Highlight new (first) vertex
					nextVertex.isHighlighted = true;
					best = nextVertex;
					bestEdge.Point2 = nextVertex;
					bestEdge.SetPosition(currVertex.transform.position, nextVertex.transform.position);
				}
				else
				{
					// Calculate vectors
					Vector3 toPrev = Vector3.Normalize(prev - currVertex.transform.position);
					Vector3 toNext = Vector3.Normalize(nextVertex.transform.position - currVertex.transform.position);
					Vector3 toBest = Vector3.Normalize(best.transform.position - currVertex.transform.position);

					// Check if angle is bigger
					if (-Vector2.Dot(toPrev, toNext) > -Vector2.Dot(toPrev, toBest))
					{
						// Highlight new vertex
						best.isHighlighted = false;
						nextVertex.isHighlighted = true;
						best = nextVertex;
						bestEdge.Point2 = nextVertex;
						bestEdge.SetPosition(currVertex.transform.position, nextVertex.transform.position);
					}
				}

				yield return new WaitForSecondsRealtime(speed);
			}

			// Move vertex pointers forward
			prevVertex = currVertex;
			prev = currVertex.transform.position;
			currVertex = best;

			// Finalize moving edge and best vertex
			currEdge.Point2 = best;
			currEdge.SetPosition(currVertex.transform.position, best.transform.position);
			currEdge.isHighlighted = false;
			best.isHighlighted = false;
		}

		// Destroy faded edge
		Destroy(bestEdge.gameObject);

		IsGenerating = false;

		yield return null;
	}
}
