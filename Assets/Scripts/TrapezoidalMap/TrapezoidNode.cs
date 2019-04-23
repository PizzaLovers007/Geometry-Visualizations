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
			if (data != value)
			{
				data = value;
				if (value)
				{
					value.Node = this;
				}
			}
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
		if (Data != null)
		{
			Destroy(Data.gameObject);
		}
		base.Clear();
	}
}
