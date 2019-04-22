using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VertexNode : Node
{
	[SerializeField]
	Vertex data;
	public Vertex Data
	{
		get { return data; }
		set { data = value; }
	}

	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{

	}

	public override bool Equals(object obj)
	{
		if (obj is VertexNode)
		{
			VertexNode other = obj as VertexNode;
			return Data.Equals(other.Data);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Data.GetHashCode();
	}
}
