using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Edge))]
public class EdgeEditor : Editor
{
	Vector2 p1Default = Vector2.zero;
	Vector2 p2Default = Vector2.zero;

	public override void OnInspectorGUI()
	{
		Edge edge = target as Edge;

		edge.defaultMaterial = EditorGUILayout.ObjectField("Default Material", edge.defaultMaterial, typeof(Material), true) as Material;
		edge.fadedMaterial = EditorGUILayout.ObjectField("Faded Material", edge.fadedMaterial, typeof(Material), true) as Material;
		edge.highlightMaterial = EditorGUILayout.ObjectField("Highlight Material", edge.highlightMaterial, typeof(Material), true) as Material;
		if (edge.Point1)
		{
			edge.Point1.transform.position = EditorGUILayout.Vector2Field("Position 1", edge.Point1.transform.position);
		}
		else
		{
			p1Default = EditorGUILayout.Vector2Field("Position 1", p1Default);
		}
		if (edge.Point2)
		{
			edge.Point2.transform.position = EditorGUILayout.Vector2Field("Position 2", edge.Point2.transform.position);
		}
		else
		{
			p2Default = EditorGUILayout.Vector2Field("Position 2", p2Default);
		}
		edge.isHighlighted = EditorGUILayout.Toggle("Is Highlighted", edge.isHighlighted);
		edge.isFaded = EditorGUILayout.Toggle("Is Faded", edge.isFaded);
	}
}
