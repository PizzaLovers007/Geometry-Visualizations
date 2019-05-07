﻿using System.Collections;
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
	public GameObject nodeLinePrefab;
	public float speed = 1;
	public float spacing = 0.3f;

	public bool IsGenerating { get; private set; }

	Node Root { get; set; }
	List<HashSet<Node>> TopologicalOrder { get; set; }
	Dictionary<Trapezoid, TrapezoidNode> TrapNodeDict { get; set; } 
	List<GameObject> NodeLines { get; set; }

	EdgeGenerator generator;

	// Use this for initialization
	void Start()
	{
		TopologicalOrder = new List<HashSet<Node>>();
		TrapNodeDict = new Dictionary<Trapezoid, TrapezoidNode>();
		NodeLines = new List<GameObject>();
		generator = GameObject.Find("EdgeGenerator").GetComponent<EdgeGenerator>();
	}

	// Update is called once per frame
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.G) && !IsGenerating)
		{
			Debug.Log("Generating map");
			if (Root)
			{
				Root.Clear();
			}
			StartCoroutine(AssembleMap(new List<Edge>(generator.Edges)));
		}
		if (Input.GetKeyDown(KeyCode.D) && !IsGenerating)
		{
			Reset();
			generator.Clear();
		}
	}

	public void Reset()
	{
		if (Root)
		{
			Root.Clear();
			TrapNodeDict.Clear();
			foreach (GameObject line in NodeLines)
			{
				Destroy(line);
			}
			NodeLines.Clear();
		}
	}

	public IEnumerator AssembleMap(List<Edge> edges)
	{
		IsGenerating = true;
		TrapNodeDict.Clear();

		// Create initial trapezoid
		Trapezoid bounds = CreateBoundingTrapezoid();
		Root = GetTrapezoidNode(bounds);
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

		ShowDAG();

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
			TrapezoidNode leftNode = LocatePointNode(leftVertex.transform.position);
			TrapezoidNode rightNode = LocatePointNode(rightVertex.transform.position);
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
				leftNode = GetTrapezoidNode(leftTrap);
				rightNode = GetTrapezoidNode(rightTrap);
				topNode = GetTrapezoidNode(topTrap);
				bottomNode = GetTrapezoidNode(bottomTrap);
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
				leftNode = GetTrapezoidNode(leftTrap);
				topNode = GetTrapezoidNode(topTrap);
				bottomNode = GetTrapezoidNode(bottomTrap);
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
						topNode = GetTrapezoidNode(topTrap);
						bottomNode = GetTrapezoidNode(bottomTrap);
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
				rightNode = GetTrapezoidNode(rightTrap);
				topNode = GetTrapezoidNode(topTrap);
				bottomNode = GetTrapezoidNode(bottomTrap);
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
			// InOrderTraverse(Root, (node) => {
			// 	if (node is TrapezoidNode)
			// 	{
			// 		sb.Append((node as TrapezoidNode).Data.id);
			// 		sb.Append(" ");
			// 	}
			// });
			// Debug.Log(sb.ToString());

			ShowDAG();

			yield return new WaitForSecondsRealtime(speed);

			currEdge.isHighlighted = false;
			currEdge.Point1.isHighlighted = false;
			currEdge.Point2.isHighlighted = false;
		}

		IsGenerating = false;

		// InOrderTraverse(Root, (node) => {
		// 	foreach (Node parent in node.Parents)
		// 	{
		// 		Debug.Log($"{parent} ======> {node}");
		// 	}
		// });

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

	TrapezoidNode GetTrapezoidNode(Trapezoid trap)
	{
		if (!TrapNodeDict.ContainsKey(trap))
		{
			TrapNodeDict[trap] = Instantiate(trapezoidNodePrefab).GetComponent<TrapezoidNode>();
			TrapNodeDict[trap].Data = trap;
		}
		return TrapNodeDict[trap];
	}

	void ShowDAG()
	{
		// Do topological ordering
		TopologicalOrder.Clear();
		foreach (GameObject line in GameObject.FindGameObjectsWithTag("NodeLine"))
		{
			Destroy(line);
		}
		HashSet<Node> currSet = new HashSet<Node>();
		Dictionary<Node, int> degree = new Dictionary<Node, int>();
		Queue<Node> queue = new Queue<Node>();
		degree[Root] = 0;

		queue.Enqueue(Root);
		while (queue.Count > 0)
		{
			Node curr = queue.Dequeue();
			if (currSet.Contains(curr))
			{
				continue;
			}
			currSet.Add(curr);
			if (curr.LeftChild)
			{
				queue.Enqueue(curr.LeftChild);
				if (!degree.ContainsKey(curr.LeftChild))
				{
					degree[curr.LeftChild] = 0;
				}
				degree[curr.LeftChild]++;
			}
			if (curr.RightChild)
			{
				queue.Enqueue(curr.RightChild);
				if (!degree.ContainsKey(curr.RightChild))
				{
					degree[curr.RightChild] = 0;
				}
				degree[curr.RightChild]++;
			}
		}

		currSet.Clear();
		currSet.Add(Root);
		while (currSet.Count > 0)
		{
			TopologicalOrder.Add(currSet);
			HashSet<Node> nextSet = new HashSet<Node>();
			foreach (Node n in currSet)
			{
				if (n.LeftChild)
				{
					degree[n.LeftChild]--;
					if (degree[n.LeftChild] == 0)
					{
						nextSet.Add(n.LeftChild);
					}
				}
				if (n.RightChild)
				{
					degree[n.RightChild]--;
					if (degree[n.RightChild] == 0)
					{
						nextSet.Add(n.RightChild);
					}
				}
			}
			currSet = nextSet;
		}
		// foreach (HashSet<Node> set in TopologicalOrder)
		// {
		// 	StringBuilder sb = new StringBuilder();
		// 	foreach (Node node in set)
		// 	{
		// 		sb.Append(node.ToString()).Append("    ");
		// 	}
		// 	Debug.Log(sb.ToString());
		// }
		// Debug.Log($"Topological Depth: {TopologicalOrder.Count}");

		// float range = Mathf.Pow(2, TopologicalOrder.Count-1) * spacing;
		// PlaceNode(Root, -range, range, 0);
		// DrawLines(Root, 0);

		Dictionary<Node, int> heights = new Dictionary<Node, int>();
		CalculateMaxHeight(Root, heights, 0);
		PlaceNode2(Root, heights, 0, 0);
		DrawLines(Root, 0);
	}

	int CalculateMaxHeight(Node currNode, Dictionary<Node, int> heights, int depth)
	{
		if (currNode == null || !TopologicalOrder[depth].Contains(currNode))
		{
			return -1;
		}
		int left = CalculateMaxHeight(currNode.LeftChild, heights, depth+1);
		int right = CalculateMaxHeight(currNode.RightChild, heights, depth+1);
		heights[currNode] = Mathf.Max(left, right) + 1;
		return heights[currNode];
	}

	void PlaceNode2(Node currNode, Dictionary<Node, int> heights, float offset, int depth)
	{
		if (TopologicalOrder[depth].Contains(currNode))
		{
			Vector3 pos = new Vector3(offset + 1000, -depth*1.2f, 0);
			currNode.transform.position = pos;
			if (currNode.LeftChild != null && currNode.RightChild != null)
			{
				// float spread = Mathf.Pow(2, (heights[currNode.LeftChild] + heights[currNode.RightChild])/2) * spacing;
				float spread = Mathf.Pow((heights[currNode.LeftChild] + heights[currNode.RightChild])/2+1, 1) * spacing;
				// float rightSpread = Mathf.Pow(2, heights[currNode.RightChild]) * spacing;
				PlaceNode2(currNode.LeftChild, heights, offset - spread, depth+1);
				PlaceNode2(currNode.RightChild, heights, offset + spread, depth+1);
			}
		}
	}

	void PlaceNode(Node currNode, float left, float right, int depth)
	{
		if (currNode && TopologicalOrder[depth].Contains(currNode))
		{
			float mid = (right + left) / 2;
			Vector3 pos = new Vector3(mid + 1000, -depth*1.2f, 0);
			// Debug.Log(pos);
			currNode.transform.position = pos;

			PlaceNode(currNode.LeftChild, left, mid, depth+1);
			PlaceNode(currNode.RightChild, mid, right, depth+1);
		}
	}

	void DrawLines(Node currNode, int depth)
	{
		if (currNode && TopologicalOrder[depth].Contains(currNode))
		{
			foreach (Node parent in currNode.Parents)
			{
				LineRenderer line = Instantiate(nodeLinePrefab).GetComponent<LineRenderer>();
				NodeLines.Add(line.gameObject);
				if (TopologicalOrder[depth-1].Contains(parent))
				{
					line.SetPosition(0, parent.transform.position);
					line.SetPosition(1, currNode.transform.position);
				}
				else
				{
					Vector3 lowerPos;
					if (parent.LeftChild == currNode)
					{
						lowerPos = parent.transform.position + new Vector3(-spacing, -1.2f, 0);
					}
					else
					{
						lowerPos = parent.transform.position + new Vector3(spacing, -1.2f, 0);
					}
					LineRenderer line2 = Instantiate(nodeLinePrefab).GetComponent<LineRenderer>();
					NodeLines.Add(line2.gameObject);
					line.SetPosition(0, parent.transform.position);
					line.SetPosition(1, lowerPos);
					line2.SetPosition(0, lowerPos);
					line2.SetPosition(1, currNode.transform.position);
				}
			}

			DrawLines(currNode.LeftChild, depth+1);
			DrawLines(currNode.RightChild, depth+1);
		}
	}

	public Trapezoid LocatePoint(Vector3 point)
	{
		if (Root == null)
		{
			return null;
		}
		return LocatePointNode(point)?.Data;
	}

	TrapezoidNode LocatePointNode(Vector3 point)
	{
		if (Root == null)
		{
			return null;
		}
		Node curr = Root;
		while (!(curr is TrapezoidNode))
		{
			if (curr == null)
			{
				return null;
			}
			if (curr is VertexNode)
			{
				Vertex other = (curr as VertexNode).Data;
				if (point.x < other.transform.position.x)
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
				if (point.y > other.CalculateYCoord(point.x))
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

	public void HighlightPath(Vector3 point, bool highlight)
	{
		if (Root == null)
		{
			return;
		}
		Node curr = Root;
		while (!(curr is TrapezoidNode))
		{
			if (curr == null)
			{
				return;
			}
			curr.isHighlighted = highlight;
			if (curr is VertexNode)
			{
				Vertex other = (curr as VertexNode).Data;
				if (point.x < other.transform.position.x)
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
				if (point.y > other.CalculateYCoord(point.x))
				{
					curr = curr.LeftChild;
				}
				else
				{
					curr = curr.RightChild;
				}
			}
		}
		curr.isHighlighted = highlight;
	}

	void InOrderTraverse(Node currNode, System.Action<Node> action)
	{
		if (currNode == null)
		{
			return;
		}
		InOrderTraverse(currNode.LeftChild, action);
		action(currNode);
		InOrderTraverse(currNode.RightChild, action);
	}
}
