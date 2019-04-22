using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
	[SerializeField]
	Node leftChild;
	public Node LeftChild
	{
		get { return leftChild; }
		set
		{
			if (leftChild)
			{
				leftChild.Parents.Remove(this);
			}
			if (value)
			{
				value.Parents.Add(this);
			}
			leftChild = value;
		}
	}
	
	[SerializeField]
	Node rightChild;
	public Node RightChild
	{
		get { return rightChild; }
		set
		{
			if (rightChild)
			{
				rightChild.Parents.Remove(this);
			}
			if (value)
			{
				value.Parents.Add(this);
			}
			rightChild = value;
		}
	}

	HashSet<Node> Parents { get; set; }

	void Awake()
	{
		Parents = new HashSet<Node>();
	}

	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{

	}

	public void ReplaceNode(Node other)
	{
		HashSet<Node> parentsCopy = new HashSet<Node>(Parents);
		foreach (Node parent in parentsCopy)
		{
			if (parent.LeftChild == this)
			{
				parent.LeftChild = other;
			}
			else
			{
				parent.RightChild = other;
			}
		}
	}

	public virtual void Clear()
	{
		if (LeftChild)
		{
			LeftChild.Clear();
			LeftChild = null;
		}
		if (RightChild)
		{
			RightChild.Clear();
			RightChild = null;
		}
		Destroy(gameObject);
	}
}
