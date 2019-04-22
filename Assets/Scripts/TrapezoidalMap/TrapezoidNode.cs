using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapezoidNode : Node
{
	[SerializeField]
	Trapezoid data;
	public Trapezoid Data
	{
		get { return data; }
		set
		{
			if (value)
			{
				value.Node = this;
			}
			data = value;
		}
	}

	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{

	}

	public override void Clear()
	{
		if (Data.gameObject != null)
		{
			Destroy(Data.gameObject);
		}
		base.Clear();
	}

	public override bool Equals(object obj)
	{
		if (obj is TrapezoidNode)
		{
			TrapezoidNode other = obj as TrapezoidNode;
			return Data.Equals(other.Data);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Data.GetHashCode();
	}
}
