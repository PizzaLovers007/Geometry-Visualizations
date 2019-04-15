using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConvexHull : MonoBehaviour {
    public float speed = 0.5f;
    public GameObject edgePrefab;

    private List<Vertex> lower;
    private List<Vertex> upper;
    private List<Vertex> hull;
    private List<Edge> edges;

    private PointCloud pointCloud;

    private void Awake() {
        lower = new List<Vertex>();
        upper = new List<Vertex>();
        hull = new List<Vertex>();

        edges = new List<Edge>();
    }

    private void Start() {
        pointCloud = GetComponent<PointCloud>();
        StartCoroutine("GrahamScan");
    }

    private void Update() {

        ConstructHull();
        for (int i = 1; i < hull.Count; i++) {
            if (i >= edges.Count) {
                Edge edge = Instantiate(edgePrefab).GetComponent<Edge>();
                edges.Add(edge);
            }
            edges[i - 1].Point1 = hull[i - 1];
            edges[i - 1].Point2 = hull[i];
            edges[i - 1].transform.SetParent(transform);
            edges[i - 1].gameObject.SetActive(true);

            edges[i - 1].isHighlighted = i >= hull.Count - 1 && edges[0].Point1 != edges[i - 1].Point2;
        }


        for (int i = hull.Count; i < edges.Count; i++) {
            edges[i].gameObject.SetActive(false);
        }
    }

    private static float Orient(Vector2 p1, Vector2 p2, Vector2 p3) {
        return (p2.x - p1.x) * (p3.y - p1.y) - (p2.y - p1.y) * (p3.x - p1.x);
    }

    private void ConstructHull() {
        hull.Clear();
        foreach (var p in upper) {
            hull.Add(p);
        }
        foreach (var p in lower) {
            hull.Add(p);
        }
    }

    private IEnumerator GrahamScan() {
        Vertex[] points = pointCloud.Points.ToArray();
        Array.Sort(points, delegate (Vertex v1, Vertex v2) {
            return v1.transform.position.x.CompareTo(v2.transform.position.x);
        });

        for (int i = 0; i < points.Length; i++) {
            while (upper.Count >= 2 && Orient(upper[upper.Count - 2].transform.position, upper[upper.Count - 1].transform.position, points[i].transform.position) >= 0) {
                upper.RemoveAt(upper.Count - 1);
                yield return new WaitForSecondsRealtime(speed);
            }

            upper.Add(points[i]);
            yield return new WaitForSecondsRealtime(speed);
        }

        for (int i = points.Length - 1; i >= 0; i--) {
            while (lower.Count >= 2 && Orient(lower[lower.Count - 2].transform.position, lower[lower.Count - 1].transform.position, points[i].transform.position) >= 0) {
                lower.RemoveAt(lower.Count - 1);
                yield return new WaitForSecondsRealtime(speed);
            }

            lower.Add(points[i]);
            yield return new WaitForSecondsRealtime(speed);
        }

        yield return null;
    }
}
