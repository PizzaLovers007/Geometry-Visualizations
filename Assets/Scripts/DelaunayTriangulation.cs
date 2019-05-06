using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Delaunay {
    public class DelaunayTriangulation : MonoBehaviour {
        private class Triangle {
            public Vertex a, b, c;

            public bool isHighlighted = false;

            public Triangle(Vertex a, Vertex b, Vertex c) {
                this.a = a;
                this.b = b;
                this.c = c;
            }

            internal bool PointInCircumcircle(Vertex point) {
                float a11 = a.transform.position.x - point.transform.position.x;
                float a21 = b.transform.position.x - point.transform.position.x;
                float a31 = c.transform.position.x - point.transform.position.x;

                float a12 = a.transform.position.y - point.transform.position.y;
                float a22 = b.transform.position.y - point.transform.position.y;
                float a32 = c.transform.position.y - point.transform.position.y;

                float a13 = a11 * a11 + a12 * a12;
                float a23 = a21 * a21 + a22 * a22;
                float a33 = a31 * a31 + a32 * a32;

                float det = a11 * a22 * a33 + a12 * a23 * a31 + a13 * a21 * a32 - a13 * a22 * a31 - a12 * a21 * a33 - a11 * a23 * a32;

                if (isOrientedCCW()) {
                    return det > 0f;
                }

                return det < 0f;
            }

            private bool isOrientedCCW() {
                float a11 = a.transform.position.x - c.transform.position.x;
                float a21 = b.transform.position.x - c.transform.position.x;

                float a12 = a.transform.position.y - c.transform.position.y;
                float a22 = b.transform.position.y - c.transform.position.y;

                double det = a11 * a22 - a12 * a21;

                return det > 0f;
            }

            internal Vertex OppositeVertex(VertexPair edge) {
                if (a != edge.a && a != edge.b) {
                    return a;
                } else if (b != edge.a && b != edge.b) {
                    return b;
                } else if (c != edge.a && c != edge.b) {
                    return c;
                }
                
                return null;
            }

            internal bool Contains(Vertex vertex) {
                float area = 0.5f * (-b.transform.position.y * c.transform.position.x + a.transform.position.y * (-b.transform.position.x + c.transform.position.x) + a.transform.position.x * (b.transform.position.y - c.transform.position.y) + b.transform.position.x * c.transform.position.y);
                float s = 1f / (2f * area) * (a.transform.position.y * c.transform.position.x - a.transform.position.x * c.transform.position.y + (c.transform.position.y - a.transform.position.y) * vertex.transform.position.x + (a.transform.position.x - c.transform.position.x) * vertex.transform.position.y);
                float t = 1f / (2f * area) * (a.transform.position.x * b.transform.position.y - a.transform.position.y * b.transform.position.x + (a.transform.position.y - b.transform.position.y) * vertex.transform.position.x + (b.transform.position.x - a.transform.position.x) * vertex.transform.position.y);

                return s > 0 && t > 0 && (1 - s - t) > 0;
            }

            internal bool IsNeighbor(VertexPair edge) {
                return (a == edge.a || b == edge.a || c == edge.a) && (a == edge.b || b == edge.b || c == edge.b);
            }

            internal bool HasVertex(Vertex v) {
                return (v == a || v == b || v == c);
            }

            public override string ToString() {
                return string.Format("[{0} {1} {2}]", a, b, c);
            }

            public bool IsTriangle() {
                return a != b && b != c && c != a;
            }
        }

        private class VertexPair {
            public Vertex a, b;

            public bool isHighlighted = false;

            public VertexPair(Vertex a, Vertex b) {
                this.a = a;
                this.b = b;
            }

            public override string ToString() {
                return string.Format("[{0}, {1}]", a, b);
            }
        }

        private class TriangleContainer {
            public List<Triangle> triangles;

            public TriangleContainer() {
                triangles = new List<Triangle>();
            }

            public void Add(Triangle triangle) {
                triangles.Add(triangle);
            }

            public void Remove(Triangle triangle) {
                triangles.Remove(triangle);
            }

            public Triangle ContainingTriangle(Vertex vertex) {
                foreach (var t in triangles) {
                    if (t.Contains(vertex)) {
                        return t;
                    }
                }

                return null;
            }

            internal Triangle FindNeighbor(Triangle triangle, VertexPair edge) {
                foreach (var t in triangles) {
                    if (t.IsNeighbor(edge) && t != triangle) {
                        return t;
                    }
                }

                return null;
            }

            public int Count { get { return triangles.Count; } }

            public Triangle this[int i] {
                get { return triangles[i]; }
            }

            public void Clear() {
                triangles.Clear();
            }

            internal void RemoveTrianglesWith(Vertex v) {
                triangles.RemoveAll(t => t.HasVertex(v));
            }
        }

        public float speed = 0.5f;

        public Vertex A;
        public Vertex B;
        public Vertex C;

        public GameObject edgePrefab;

        public bool IsGenerating { get; private set; }

        private PointCloud pointCloud;
        private VertexGenerator generator;

        private TriangleContainer container;

        private List<Edge> edges;
        private List<Vertex> added;

        private void Start() {
            pointCloud = GameObject.Find("PointCloud").GetComponent<PointCloud>();
            generator = GameObject.Find("VertexGenerator").GetComponent<VertexGenerator>();
            edges = new List<Edge>();
            container = new TriangleContainer();
            added = new List<Vertex>();
        }

        private void Update() {
            if (Input.GetKeyDown(KeyCode.D) && !IsGenerating) {
                foreach (Edge e in edges) {
                    e.gameObject.SetActive(true);
                    Destroy(e.gameObject);
                }
                edges.Clear();
                generator.Clear();
                container.Clear();
                added.Clear();
            }
            if (Input.GetKeyDown(KeyCode.R) && !IsGenerating) {
                pointCloud.GenerateCloud();
            }

            if (Input.GetKeyDown(KeyCode.G) && !IsGenerating) {
                edges.Clear();
                container.Clear();
                added.Clear();
                StartCoroutine(Triangulate());
            }

            Render();
        }

        private void Render() {
            for (int p = 0; p < generator.IterableVertices.Count; p++) {
                generator.IterableVertices[p].isFaded = true;
            }

            for (int p = 0; p < added.Count; p++) {
                added[p].isFaded = false;
            }

            List<VertexPair> vertexPairs = new List<VertexPair>();
            for (int t = 0; t < container.Count; t++) {
                Triangle tri = container[t];
                var e1 = new VertexPair(tri.a, tri.b);
                var e2 = new VertexPair(tri.b, tri.c);
                var e3 = new VertexPair(tri.c, tri.a);
                if (tri.isHighlighted) {
                    e1.isHighlighted = true;
                    e2.isHighlighted = true;
                    e3.isHighlighted = true;
                }

                vertexPairs.Add(e1);
                vertexPairs.Add(e2);
                vertexPairs.Add(e3);
            }

            for (int i = 0; i < vertexPairs.Count; i++) {
                if (i >= edges.Count) {
                    Edge edge = Instantiate(edgePrefab).GetComponent<Edge>();
                    edges.Add(edge);
                }

                edges[i].Point1 = vertexPairs[i].a;
                edges[i].Point2 = vertexPairs[i].b;
                edges[i].transform.SetParent(transform);
                edges[i].isHighlighted = vertexPairs[i].isHighlighted;

                edges[i].gameObject.SetActive(true);
            }

            for (int i = vertexPairs.Count; i < edges.Count; i++) {
                edges[i].gameObject.SetActive(false);
            }
        }

        private IEnumerator Triangulate() {
            IsGenerating = true;

            // create super triangle
            Triangle super = CreateSuperTriangle();

            // add triangle
            container.Add(super);

            for (int p = 0; p < generator.IterableVertices.Count; p++) {
                Vertex point = generator.IterableVertices[p];
                point.isHighlighted = true;
                added.Add(point);

                yield return new WaitForSecondsRealtime(speed);

                // get containing triangle
                Triangle t = container.ContainingTriangle(point);

                t.isHighlighted = true;

                yield return new WaitForSecondsRealtime(speed);

                t.isHighlighted = false;

                if (t != null) {
                    // get points of triangle
                    Vertex a = t.a;
                    Vertex b = t.b;
                    Vertex c = t.c;

                    // remove triangle
                    container.Remove(t);

                    // create new triangles
                    Triangle t1 = new Triangle(a, b, point);
                    Triangle t2 = new Triangle(b, c, point);
                    Triangle t3 = new Triangle(c, a, point);

                    // add triangles
                    container.Add(t1);
                    container.Add(t2);
                    container.Add(t3);

                    yield return new WaitForSecondsRealtime(speed);

                    // make legal
                    yield return MakeDelaunay(t1, new VertexPair(a, b), point);
                    yield return MakeDelaunay(t2, new VertexPair(b, c), point);
                    yield return MakeDelaunay(t3, new VertexPair(c, a), point);

                    yield return new WaitForSecondsRealtime(speed);
                } else {
                    Debug.Log("Not contained!");
                }

                point.isHighlighted = false;
            }

            // remove edges for super triangle
            container.RemoveTrianglesWith(A);
            container.RemoveTrianglesWith(B);
            container.RemoveTrianglesWith(C);

            IsGenerating = false;

            yield return null;
        }

        private Triangle CreateSuperTriangle() {
            return new Triangle(A, B, C);
        }

        private IEnumerator MakeDelaunay(Triangle triangle, VertexPair edge, Vertex point) {
            Triangle neighbor = container.FindNeighbor(triangle, edge);

            if (neighbor != null) {
                if (neighbor.PointInCircumcircle(point)) {
                    triangle.isHighlighted = true;
                    neighbor.isHighlighted = true;

                    yield return new WaitForSecondsRealtime(speed);

                    container.Remove(triangle);
                    container.Remove(neighbor);

                    Vertex n = neighbor.OppositeVertex(edge);
                    

                    Triangle t1 = new Triangle(n, edge.a, point);
                    Triangle t2 = new Triangle(n, edge.b, point);
                    t1.isHighlighted = true;
                    t2.isHighlighted = true;

                    container.Add(t1);
                    container.Add(t2);
                    
                    yield return new WaitForSecondsRealtime(speed);

                    t1.isHighlighted = false;
                    t2.isHighlighted = false;

                    yield return MakeDelaunay(t1, new VertexPair(n, edge.a), point);
                    yield return MakeDelaunay(t2, new VertexPair(n, edge.b), point);
                }
            } else {
                //Debug.Log("No neighbor!");
            }

            yield return null;
        }
    }
}