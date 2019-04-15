using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointCloud : MonoBehaviour {
    public int Size = 20;

    public GameObject vertexPrefab;

    public List<Vertex> Points { get; private set; }
    
    private void Awake() {
        Points = new List<Vertex>();
    }

    private void Start() {
        GenerateCloud();
    }

    private void Update() {
        
    }

    private void GenerateCloud() {
        Random.InitState(10);
        float radius = 5f;

        float[] densityZones = new float[] { 1f };

        for (int i = 0; i < Size; i++) {
            int zone = 0;
            float f = Random.Range(0, 1);
            float t = 0;
            for (int d = 0; d < densityZones.Length; d++) {
                t += densityZones[d];
                if (f <= t) {
                    zone = d;
                    break;
                }
            }

            float[] range = new float[] {
                (((float)zone) / densityZones.Length) * radius,
                (((float)zone + 1) / densityZones.Length) * radius,
            };

            float angle = Random.Range(0f, 360f);

            Vector2 direction = Quaternion.AngleAxis(angle, Vector3.back) * Vector2.up;
            Vector2 position = direction * Random.Range(range[0], range[1]);

            Vertex vert = Instantiate(vertexPrefab, position, Quaternion.identity).GetComponent<Vertex>();
            vert.transform.SetParent(transform);
            Points.Add(vert);
        }
    }
}
