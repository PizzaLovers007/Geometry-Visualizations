using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EdgeNode : Node
{
	[SerializeField]
	Edge data;
	public Edge Data
	{
		get { return data; }
		set { data = value; }
	}
}
