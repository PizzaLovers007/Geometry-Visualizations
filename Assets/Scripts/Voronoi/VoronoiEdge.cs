using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class VoronoiEdge : MonoBehaviour {

    public Material defaultMaterial;
    LineRenderer lineRenderer;
    int id;

    static int edgeCount = 0;

    void Awake () {
        id = edgeCount;
        edgeCount++;

        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.startColor = Color.black;
        lineRenderer.endColor = Color.black;
        lineRenderer.material = defaultMaterial;
    }

    public void SetPosition(Vector2 pos1, Vector2 pos2)
    {
        // Set line position
        lineRenderer.SetPosition(0, pos1);
        lineRenderer.SetPosition(1, pos2);
    }

    public override int GetHashCode()
    {
        return id;
    }
}
