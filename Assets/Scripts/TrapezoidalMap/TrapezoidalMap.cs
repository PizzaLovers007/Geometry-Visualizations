using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class TrapezoidalMap : MonoBehaviour
{
	public GameObject vertexPrefab;
	public GameObject edgePrefab;
	public GameObject trapezoidPrefab;
	public GameObject vertexNodePrefab;
	public GameObject edgeNodePrefab;
	public GameObject trapezoidNodePrefab;
	public float speed = 1;

	Node Root { get; set; }

	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.G))
		{
			Debug.Log("Generating map");
			if (Root)
			{
				Root.Clear();
			}
			EdgeGenerator generator = GameObject.Find("EdgeGenerator").GetComponent<EdgeGenerator>();
			StartCoroutine(AssembleMap(new List<Edge>(generator.Edges)));
		}
	}

	public IEnumerator AssembleMap(List<Edge> edges)
	{
		// Create initial trapezoid
		Trapezoid bounds = CreateBoundingTrapezoid();
		Root = Instantiate(trapezoidNodePrefab).GetComponent<TrapezoidNode>();
		(Root as TrapezoidNode).Data = bounds;
		Root.gameObject.name = "Root";
		
		// Shuffle edges
		for (int i = 0; i < edges.Count; i++)
		{
			// Swap random
			int swapIndex = Random.Range(i, edges.Count);
			Edge temp = edges[i];
			edges[i] = edges[swapIndex];
			edges[swapIndex] = temp;

			// Fade edge
			edges[i].isFaded = true;
			edges[i].Point1.isFaded = true;
			edges[i].Point2.isFaded = true;
		}

		yield return new WaitForSecondsRealtime(speed);

		// Insert each edge
		foreach (Edge currEdge in edges)
		{
			currEdge.isFaded = false;
			currEdge.Point1.isFaded = false;
			currEdge.Point2.isFaded = false;
			currEdge.isHighlighted = true;
			currEdge.Point1.isHighlighted = true;
			currEdge.Point2.isHighlighted = true;

			// Get left and right vertices
			Vertex leftVertex = currEdge.LeftVertex;
			Vertex rightVertex = currEdge.RightVertex;

			// Get left and right endpoint trapezoids
			TrapezoidNode leftNode = LocatePointNode(leftVertex);
			TrapezoidNode rightNode = LocatePointNode(rightVertex);
			Trapezoid leftTrap = leftNode.Data;
			Trapezoid rightTrap = rightNode.Data;

			// Find the trapezoids the current edge touches
			List<Vertex> cutVertices = new List<Vertex>();
			List<Trapezoid> edgeTraps = new List<Trapezoid>();
			edgeTraps.Add(leftTrap);
			Trapezoid stepTrap = leftTrap;
			while (stepTrap != rightTrap)
			{
				cutVertices.Add(stepTrap.RightVertex);
				stepTrap = stepTrap.FindRightTrapezoid(currEdge);
				edgeTraps.Add(stepTrap);
			}

			VertexNode pNode, qNode;
			EdgeNode sNode;
			TrapezoidNode topNode, bottomNode;
			Trapezoid topTrap, bottomTrap;
			// Check if edge contained in trapezoid entirely
			if (edgeTraps.Count == 1)
			{
				// Split into 4 trapezoids
				leftTrap = Instantiate(trapezoidPrefab).GetComponent<Trapezoid>();
				leftTrap.TopEdge = edgeTraps.First().TopEdge;
				leftTrap.BottomEdge = edgeTraps.First().BottomEdge;
				leftTrap.LeftVertex = edgeTraps.First().LeftVertex;
				leftTrap.RightVertex = leftVertex;
				topTrap = Instantiate(trapezoidPrefab).GetComponent<Trapezoid>();
				topTrap.TopEdge = edgeTraps.First().TopEdge;
				topTrap.BottomEdge = currEdge;
				topTrap.LeftVertex = leftVertex;
				topTrap.RightVertex = rightVertex;
				bottomTrap = Instantiate(trapezoidPrefab).GetComponent<Trapezoid>();
				bottomTrap.TopEdge = currEdge;
				bottomTrap.BottomEdge = edgeTraps.First().BottomEdge;
				bottomTrap.LeftVertex = leftVertex;
				bottomTrap.RightVertex = rightVertex;
				rightTrap = Instantiate(trapezoidPrefab).GetComponent<Trapezoid>();
				rightTrap.TopEdge = edgeTraps.First().TopEdge;
				rightTrap.BottomEdge = edgeTraps.First().BottomEdge;
				rightTrap.LeftVertex = rightVertex;
				rightTrap.RightVertex = edgeTraps.First().RightVertex;

				// Link trapezoids
				leftTrap.TopRightTrap = topTrap;
				leftTrap.BottomRightTrap = bottomTrap;
				rightTrap.TopLeftTrap = topTrap;
				rightTrap.BottomLeftTrap = bottomTrap;
				leftTrap.TopLeftTrap = edgeTraps.First().TopLeftTrap;
				leftTrap.BottomLeftTrap = edgeTraps.First().BottomLeftTrap;
				rightTrap.TopRightTrap = edgeTraps.First().TopRightTrap;
				rightTrap.BottomRightTrap = edgeTraps.First().BottomRightTrap;

				// Create new DAG nodes
				leftNode = Instantiate(trapezoidNodePrefab).GetComponent<TrapezoidNode>();
				leftNode.Data = leftTrap;
				rightNode = Instantiate(trapezoidNodePrefab).GetComponent<TrapezoidNode>();
				rightNode.Data = rightTrap;
				topNode = Instantiate(trapezoidNodePrefab).GetComponent<TrapezoidNode>();
				topNode.Data = topTrap;
				bottomNode = Instantiate(trapezoidNodePrefab).GetComponent<TrapezoidNode>();
				bottomNode.Data = bottomTrap;
				pNode = Instantiate(vertexNodePrefab).GetComponent<VertexNode>();
				pNode.Data = leftVertex;
				qNode = Instantiate(vertexNodePrefab).GetComponent<VertexNode>();
				qNode.Data = rightVertex;
				sNode = Instantiate(edgeNodePrefab).GetComponent<EdgeNode>();
				sNode.Data = currEdge;

				// Setup subtree
				pNode.LeftChild = leftNode;
				pNode.RightChild = qNode;
				qNode.LeftChild = sNode;
				qNode.RightChild = rightNode;
				sNode.LeftChild = topNode;
				sNode.RightChild = bottomNode;

				// Fix tree
				edgeTraps.First().Node.ReplaceNode(pNode);
				if (edgeTraps.First().Node == Root)
				{
					Root = pNode;
					Root.gameObject.name = "Root";
				}
				
				// Destroy old trapezoid
				// Debug.Log($"Deleting trapezoid {edgeTraps.First().id}");
				Destroy(edgeTraps.First().Node.gameObject);
				Destroy(edgeTraps.First().gameObject);
				// edgeTraps.First().Node.gameObject.SetActive(false);
				// edgeTraps.First().gameObject.SetActive(false);
			}
			else
			{
				// Create trapezoids left of edge
				leftTrap = Instantiate(trapezoidPrefab).GetComponent<Trapezoid>();
				leftTrap.TopEdge = edgeTraps.First().TopEdge;
				leftTrap.BottomEdge = edgeTraps.First().BottomEdge;
				leftTrap.LeftVertex = edgeTraps.First().LeftVertex;
				leftTrap.RightVertex = leftVertex;
				topTrap = Instantiate(trapezoidPrefab).GetComponent<Trapezoid>();
				topTrap.TopEdge = edgeTraps.First().TopEdge;
				topTrap.BottomEdge = currEdge;
				topTrap.LeftVertex = leftVertex;
				bottomTrap = Instantiate(trapezoidPrefab).GetComponent<Trapezoid>();
				bottomTrap.TopEdge = currEdge;
				bottomTrap.BottomEdge = edgeTraps.First().BottomEdge;
				bottomTrap.LeftVertex = leftVertex;

				// Link trapezoids
				leftTrap.TopRightTrap = topTrap;
				leftTrap.BottomRightTrap = bottomTrap;
				leftTrap.TopLeftTrap = edgeTraps.First().TopLeftTrap;
				leftTrap.BottomLeftTrap = edgeTraps.First().BottomLeftTrap;

				// Create corresponding DAG nodes
				leftNode = Instantiate(trapezoidNodePrefab).GetComponent<TrapezoidNode>();
				leftNode.Data = leftTrap;
				topNode = Instantiate(trapezoidNodePrefab).GetComponent<TrapezoidNode>();
				topNode.Data = topTrap;
				bottomNode = Instantiate(trapezoidNodePrefab).GetComponent<TrapezoidNode>();
				bottomNode.Data = bottomTrap;
				pNode = Instantiate(vertexNodePrefab).GetComponent<VertexNode>();
				pNode.Data = leftVertex;
				sNode = Instantiate(edgeNodePrefab).GetComponent<EdgeNode>();
				sNode.Data = currEdge;

				// Setup subtree
				pNode.LeftChild = leftNode;
				pNode.RightChild = sNode;
				sNode.LeftChild = topNode;
				sNode.RightChild = bottomNode;

				// Fix tree
				edgeTraps.First().Node.ReplaceNode(pNode);
				if (edgeTraps.First().Node == Root)
				{
					Root = pNode;
					Root.gameObject.name = "Root";
				}

				// Create the middle trapezoids
				for (int i = 1; i < edgeTraps.Count; i++)
				{
					Vertex currVertex = cutVertices[i-1];
					Trapezoid currTrap = edgeTraps[i];
					Trapezoid prevTrap = edgeTraps[i-1];
					TrapezoidNode currNode = currTrap.Node;

					Trapezoid newTrap;
					// If vertex is above
					if (currVertex.transform.position.y > currEdge.CalculateYCoord(currVertex.transform.position.x))
					{
						// Finish top trapezoid
						topTrap.RightVertex = currVertex;

						// Create new top trapezoid
						newTrap = Instantiate(trapezoidPrefab).GetComponent<Trapezoid>();
						newTrap.TopEdge = currTrap.TopEdge;
						newTrap.BottomEdge = currEdge;
						newTrap.LeftVertex = currVertex;

						// Link trapezoids
						if (currTrap.TopEdge.LeftVertex == currVertex)
						{
							topTrap.TopRightTrap = prevTrap.TopRightTrap;
						}
						else
						{
							newTrap.TopLeftTrap = currTrap.TopLeftTrap;
						}
						topTrap.BottomRightTrap = newTrap;

						// Move top to next trapezoid
						topTrap = newTrap;
					}
					// Otherwise vertex is below
					else
					{
						// Finish bottom trapezoid
						bottomTrap.RightVertex = currVertex;

						// Create new bottom trapezoid
						newTrap = Instantiate(trapezoidPrefab).GetComponent<Trapezoid>();
						newTrap.TopEdge = currEdge;
						newTrap.BottomEdge = currTrap.BottomEdge;
						newTrap.LeftVertex = currVertex;

						// Link trapezoids
						if (currTrap.BottomEdge.LeftVertex == currVertex)
						{
							bottomTrap.BottomRightTrap = prevTrap.BottomRightTrap;
						}
						else
						{
							newTrap.BottomLeftTrap = currTrap.BottomLeftTrap;
						}
						bottomTrap.TopRightTrap = newTrap;

						// Move bottom to next trapezoid
						bottomTrap = newTrap;
					}
					
					if (i != edgeTraps.Count-1)
					{
						// Create corresponding DAG nodes
						topNode = Instantiate(trapezoidNodePrefab).GetComponent<TrapezoidNode>();
						topNode.Data = topTrap;
						bottomNode = Instantiate(trapezoidNodePrefab).GetComponent<TrapezoidNode>();
						bottomNode.Data = bottomTrap;
						sNode = Instantiate(edgeNodePrefab).GetComponent<EdgeNode>();
						sNode.Data = currEdge;
						
						// Setup subtree
						sNode.LeftChild = topNode;
						sNode.RightChild = bottomNode;

						// Fix tree
						currNode.ReplaceNode(sNode);
						if (currNode == Root)
						{
							Root = sNode;
							Root.gameObject.name = "Root";
						}
					}
				}

				// Finish top and bottom trapezoids
				topTrap.RightVertex = rightVertex;
				bottomTrap.RightVertex = rightVertex;

				// Create trapezoid right of edge
				rightTrap = Instantiate(trapezoidPrefab).GetComponent<Trapezoid>();
				rightTrap.TopEdge = edgeTraps.Last().TopEdge;
				rightTrap.BottomEdge = edgeTraps.Last().BottomEdge;
				rightTrap.LeftVertex = rightVertex;
				rightTrap.RightVertex = edgeTraps.Last().RightVertex;

				// Link trapezoids
				rightTrap.TopLeftTrap = topTrap;
				rightTrap.BottomLeftTrap = bottomTrap;
				rightTrap.TopRightTrap = edgeTraps.Last().TopRightTrap;
				rightTrap.BottomRightTrap = edgeTraps.Last().BottomRightTrap;

				// Create corresponding DAG nodes
				rightNode = Instantiate(trapezoidNodePrefab).GetComponent<TrapezoidNode>();
				rightNode.Data = rightTrap;
				topNode = Instantiate(trapezoidNodePrefab).GetComponent<TrapezoidNode>();
				topNode.Data = topTrap;
				bottomNode = Instantiate(trapezoidNodePrefab).GetComponent<TrapezoidNode>();
				bottomNode.Data = bottomTrap;
				pNode = Instantiate(vertexNodePrefab).GetComponent<VertexNode>();
				pNode.Data = rightVertex;
				sNode = Instantiate(edgeNodePrefab).GetComponent<EdgeNode>();
				sNode.Data = currEdge;

				// Setup subtree
				pNode.LeftChild = sNode;
				pNode.RightChild = rightNode;
				sNode.LeftChild = topNode;
				sNode.RightChild = bottomNode;

				// Fix tree
				edgeTraps.Last().Node.ReplaceNode(pNode);
				if (edgeTraps.Last().Node == Root)
				{
					Root = pNode;
					Root.gameObject.name = "Root";
				}
				
				// Destroy old trapezoids
				for (int i = 0; i < edgeTraps.Count; i++)
				{
					// Debug.Log($"Deleting trapezoid {edgeTraps[i].id}");
					Destroy(edgeTraps[i].Node.gameObject);
					Destroy(edgeTraps[i].gameObject);
					// edgeTraps[i].Node.gameObject.SetActive(false);
					// edgeTraps[i].gameObject.SetActive(false);
				}
			}
			
			// StringBuilder sb = new StringBuilder();
			// InOrderTraverse(sb, Root);
			// Debug.Log(sb.ToString());

			yield return new WaitForSecondsRealtime(speed);

			currEdge.isHighlighted = false;
			currEdge.Point1.isHighlighted = false;
			currEdge.Point2.isHighlighted = false;
		}
		yield return null;
	}

	Trapezoid CreateBoundingTrapezoid()
	{
		// Find min/max coordinates
		float minY = -Camera.main.orthographicSize;
		float maxY = Camera.main.orthographicSize;
		float minX = -Camera.main.orthographicSize * Camera.main.aspect;
		float maxX = Camera.main.orthographicSize * Camera.main.aspect;
		minY--;
		maxY++;
		minX--;
		maxX++;

		// Create vertices
		Vertex topLeft = Instantiate(vertexPrefab).GetComponent<Vertex>();
		Vertex topRight = Instantiate(vertexPrefab).GetComponent<Vertex>();
		Vertex bottomLeft = Instantiate(vertexPrefab).GetComponent<Vertex>();
		Vertex bottomRight = Instantiate(vertexPrefab).GetComponent<Vertex>();
		topLeft.transform.position = new Vector2(minX, maxY);
		topRight.transform.position = new Vector2(maxX, maxY);
		bottomLeft.transform.position = new Vector2(minX, minY);
		bottomRight.transform.position = new Vector2(maxX, minY);

		// Create edges
		Edge topEdge = Instantiate(edgePrefab).GetComponent<Edge>();
		Edge bottomEdge = Instantiate(edgePrefab).GetComponent<Edge>();
		topEdge.Point1 = topLeft;
		topEdge.Point2 = topRight;
		bottomEdge.Point1 = bottomLeft;
		bottomEdge.Point2 = bottomRight;

		// Create trapezoid
		Trapezoid trap = Instantiate(trapezoidPrefab).GetComponent<Trapezoid>();
		trap.TopEdge = topEdge;
		trap.BottomEdge = bottomEdge;
		trap.LeftVertex = topEdge.Point1;
		trap.RightVertex = topEdge.Point2;

		return trap;
	}

	public Trapezoid LocatePoint(Vertex point)
	{
		return LocatePointNode(point).Data;
	}

	TrapezoidNode LocatePointNode(Vertex point)
	{
		Node curr = Root;
		while (!(curr is TrapezoidNode))
		{
			if (curr is VertexNode)
			{
				Vertex other = (curr as VertexNode).Data;
				if (point.transform.position.x < other.transform.position.x)
				{
					curr = curr.LeftChild;
				}
				else
				{
					curr = curr.RightChild;
				}
			}
			else
			{
				Edge other = (curr as EdgeNode).Data;
				if (point.transform.position.y > other.CalculateYCoord(point.transform.position.x))
				{
					curr = curr.LeftChild;
				}
				else
				{
					curr = curr.RightChild;
				}
			}
		}
		return curr as TrapezoidNode;
	}

	void InOrderTraverse(StringBuilder sb, Node currNode)
	{
		if (currNode == null)
		{
			return;
		}
		InOrderTraverse(sb, currNode.LeftChild);
		if (currNode is TrapezoidNode)
		{
			Trapezoid trap = (currNode as TrapezoidNode).Data;

			sb.Append(trap.id).Append(" ");
		}
		InOrderTraverse(sb, currNode.RightChild);
	}
}
