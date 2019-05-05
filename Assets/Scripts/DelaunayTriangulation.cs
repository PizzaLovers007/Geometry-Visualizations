using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//public class Triangle {
//    public Vertex v1;
//    public Vertex v2;
//    public Vertex v3;

//    public Edge e12;
//    public Edge e23;
//    public Edge e31;

//    public Triangle(Vertex v1, Vertex v2, Vertex v3, Edge e12, Edge e23, Edge e31) {
//        this.v1 = v1;
//        this.v2 = v2;
//        this.v3 = v3;
//    }

//    public bool PointInTriangle(Vertex v) {
//        float area = 0.5f * (-v2.transform.position.y * v3.transform.position.x + v1.transform.position.y * (-v2.transform.position.x + v3.transform.position.x) + v1.transform.position.x * (v2.transform.position.y - v3.transform.position.y) + v2.transform.position.x * v3.transform.position.y);
//        float s = 1f / (2f * area) * (v1.transform.position.y * v3.transform.position.x - v1.transform.position.x * v3.transform.position.y + (v3.transform.position.y - v1.transform.position.y) * v.transform.position.x + (v1.transform.position.x - v3.transform.position.x) * v.transform.position.y);
//        float t = 1f / (2f * area) * (v1.transform.position.x * v2.transform.position.y - v1.transform.position.y * v2.transform.position.x + (v1.transform.position.y - v2.transform.position.y) * v.transform.position.x + (v2.transform.position.x - v1.transform.position.x) * v.transform.position.y);

//        return s > 0 && t > 0 && (1 - s - t) > 0;
//    }

//    private bool InCircumcircle(Vertex v) {
//        var a = v1.transform.position.x - v.transform.position.x;
//        var b = v1.transform.position.y - v.transform.position.y;
//        var c = a * a + b * b;

//        var d = v2.transform.position.x - v.transform.position.x;
//        var e = v2.transform.position.y - v.transform.position.y;
//        var f = d * d + e * e;

//        var g = v3.transform.position.x - v.transform.position.x;
//        var h = v3.transform.position.y - v.transform.position.y;
//        var i = g * g + h * h;
//        var det = a * (e * i - f * h) - b * (d * i - f * g) + c * (d * h - e * g);
//        return det > 0;
//    }

//    private Vector3 LineLineIntersection(Vector3 origin1, Vector3 direction1, Vector3 origin2, Vector3 direction2) {
//        direction1.Normalize();
//        direction2.Normalize();
//        var N = Vector3.Cross(direction1, direction2);
//        var SR = origin1 - origin2;
//        var absX = Mathf.Abs(N.x);
//        var absY = Mathf.Abs(N.y);
//        var absZ = Mathf.Abs(N.z);
//        float t;
//        if (absZ > absX && absZ > absY) {
//            t = (SR.x * direction2.y - SR.y * direction2.x) / N.z;
//        } else if (absX > absY) {
//            t = (SR.y * direction2.z - SR.z * direction2.y) / N.x;
//        } else {
//            t = (SR.z * direction2.x - SR.x * direction2.z) / N.y;
//        }
//        return origin1 - t * direction1;
//    }

//    public bool IsDelaunay(Vertex[] vertices, out Vertex[] violators) {
//        List<Vertex> violating_vertices = new List<Vertex>();
//        bool result = true;
//        foreach (var v in vertices) {
//            if (Contains(v)) {
//                continue;
//            }

//            if (InCircumcircle(v)) {
//                violating_vertices.Add(v);
//                result = false;
//            }
//        }

//        violators = violating_vertices.ToArray();
//        return result;
//    }

//    public bool Contains(Vertex v) {
//        return (v == v1 || v == v2 || v == v3);
//    }

//    public bool Contains(Edge e) {
//        return (e == e12 || e == e23 || e == e31);
//    }

//    public Triangle[] getNeighbors(Triangle[] candidates) {
//        List<Triangle> neighbors = new List<Triangle>();
//        foreach(var t in candidates) {
//            if(IsNeighbor(t)) {
//                neighbors.Add(t);
//            }
//        }

//        return neighbors.ToArray();
//    }

//    public bool IsNeighbor(Triangle other) {
//        return Contains(e12) || Contains(e23) || Contains(e31);
//    }

//    public override int GetHashCode() {
//        return base.GetHashCode();
//    }

//    public override bool Equals(object obj) {
//        if(obj is Triangle) {
//            Triangle other = obj as Triangle;
//            return other.Contains(v1) && other.Contains(v2) && other.Contains(v3);
//        }

//        return false;
//    }

//    public static void OppositePoint(Triangle triangle1, Triangle triangle2, out Vertex o1, out Vertex o2) {
//        o1 = null;
//        o2 = null;

//        if (!triangle1.IsNeighbor(triangle2)) {
//            return;
//        }

//        if(triangle2.Contains(triangle1.e12)) {
//            o1 = triangle1.v3;
//        } else if(triangle2.Contains(triangle1.e23)) {
//            o1 = triangle1.v1;
//        } else if(triangle2.Contains(triangle1.e31)) {
//            o1 = triangle1.v2;
//        }

//        if (triangle1.Contains(triangle2.e12)) {
//            o2 = triangle2.v3;
//        } else if(triangle1.Contains(triangle2.e23)) {
//            o2 = triangle2.v1;
//        } else if(triangle1.Contains(triangle2.e31)) {
//            o2 = triangle2.v2;
//        }

//        return;
//    }
//}

//struct EdgeStruct {
//    public Vertex p1, p2;
//}

//struct TriangleStruct {
//    public Vertex p1, p2, p3;
//}

namespace Delaunay {
    public class DelaunayTriangulation : MonoBehaviour {
        private class Triangle {
            public Vertex a, b, c;

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

                if(isOrientedCCW()) {
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

            internal Vertex OppositeVertex(Edge edge) {
                if(a!=edge.a && a!=edge.b) {
                    return a;
                } else if (b != edge.a && b != edge.b) {
                    return b;
                }
                else if(c != edge.a && c != edge.b) {
                    return a;
                }

                return null;
            }

            internal bool Contains(Vertex vertex) {
                float area = 0.5f * (-b.transform.position.y * c.transform.position.x + a.transform.position.y * (-b.transform.position.x + c.transform.position.x) + a.transform.position.x * (b.transform.position.y - c.transform.position.y) + b.transform.position.x * c.transform.position.y);
                float s = 1f / (2f * area) * (a.transform.position.y * c.transform.position.x - a.transform.position.x * c.transform.position.y + (c.transform.position.y - a.transform.position.y) * vertex.transform.position.x + (a.transform.position.x - c.transform.position.x) * vertex.transform.position.y);
                float t = 1f / (2f * area) * (a.transform.position.x * b.transform.position.y - a.transform.position.y * b.transform.position.x + (a.transform.position.y - b.transform.position.y) * vertex.transform.position.x + (b.transform.position.x - a.transform.position.x) * vertex.transform.position.y);

                return s > 0 && t > 0 && (1 - s - t) > 0;
            }

            internal bool IsNeighbor(Edge edge) {
                return (a == edge.a || b == edge.a || c == edge.a) && (a == edge.b || b == edge.b || c == edge.b); 
            }
        }

        private class Edge {
            public Vertex a, b;

            public Edge(Vertex a, Vertex b) {
                this.a = a;
                this.b = b;
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
                foreach(var t in triangles) {
                    if(t.Contains(vertex)) {
                        return t;
                    }
                }

                return null;
            }

            internal Triangle FindNeighbor(Triangle triangle, Edge edge) {
                foreach(var t in triangles) {
                    if(t.IsNeighbor(edge) && t != triangle) {
                        return t;
                    }
                }

                return null;
            }
        }

        public Vertex A;
        public Vertex B;
        public Vertex C;

        private List<Vertex> points;
        private TriangleContainer container;

        private void Start() {
            StartCoroutine("Triangulate");
        }

        private IEnumerator Triangulate() {
            // create super triangle
            Triangle super = CreateSuperTriangle();

            // add triangle
            container.Add(super);

            for (int p = 0; p < points.Count; p++) {
                // get containing triangle
                Triangle t = container.ContainingTriangle(points[p]);

                // get points of triangle
                Vertex a = t.a;
                Vertex b = t.b;
                Vertex c = t.c;

                // remove triangle
                container.Remove(t);

                // create new triangles
                Triangle t1 = new Triangle(a, b, points[p]);
                Triangle t2 = new Triangle(b, c, points[p]);
                Triangle t3 = new Triangle(c, a, points[p]);

                // add triangles
                container.Add(t1);
                container.Add(t2);
                container.Add(t3);

                // make legal
                MakeDelaunay(t1, new Edge(a, b), points[p]);
                MakeDelaunay(t2, new Edge(b, c), points[p]);
                MakeDelaunay(t3, new Edge(c, a), points[p]);
            }

            // remove edges for super triangle

            yield return null;
        }

        private Triangle CreateSuperTriangle() {
            return new Triangle(A, B, C);
        }

        private void MakeDelaunay(Triangle triangle, Edge edge, Vertex point) {
            Triangle neighbor = container.FindNeighbor(triangle, edge);

            if (neighbor != null) {
                if (neighbor.PointInCircumcircle(point)) {
                    container.Remove(triangle);
                    container.Remove(neighbor);

                    Vertex n = neighbor.OppositeVertex(edge);

                    Triangle t1 = new Triangle(n, edge.a, point);
                    Triangle t2 = new Triangle(n, edge.b, point);

                    container.Add(t1);
                    container.Add(t2);

                    MakeDelaunay(t1, new Edge(n, edge.a), point);
                    MakeDelaunay(t2, new Edge(n, edge.b), point);
                }
            }
        }
    }
}