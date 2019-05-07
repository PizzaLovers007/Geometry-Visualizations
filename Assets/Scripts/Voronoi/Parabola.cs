using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parabola : MonoBehaviour {

    public Material defaultMaterial;
    private const int parabolaSegments = 200;
    LineRenderer lineRenderer;
    int id;

    static int parabolaCount = 0;

    void Awake()
    {
        id = parabolaCount;
        parabolaCount++;

        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.startColor = Color.black;
        lineRenderer.endColor = Color.black;
        lineRenderer.material = defaultMaterial;
    }

    public void CalculateParabola(Site site, float directrix)
    {
        // If directrix is above the site
        if(directrix > site.transform.position.y)
        {
            lineRenderer.enabled = true;
            float segmentLength = ((float)Screen.width) / ((float)parabolaSegments);
            Vector3[] parabolaVertexList = new Vector3[parabolaSegments];
            lineRenderer.positionCount = parabolaSegments;
            for (int x = 0; x < parabolaSegments; x++)
            {
                float worldXCoord = Camera.main.ScreenToWorldPoint(new Vector3(x * segmentLength, 0.0f)).x;
                float yPos = ((1 / (2 * (site.transform.position.y - directrix))) * Mathf.Pow(worldXCoord - site.transform.position.x, 2)) + ((site.transform.position.y + directrix) / 2);
                parabolaVertexList[x] = new Vector3(worldXCoord, yPos);
            }
            lineRenderer.SetPositions(parabolaVertexList);
        }
        // If directrix is at the site
        else if (directrix == site.transform.position.y)
        {
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, new Vector3(site.transform.position.x, site.transform.position.y));
            lineRenderer.SetPosition(1, new Vector3(site.transform.position.x, Camera.main.ScreenToWorldPoint(new Vector3(0.0f, 0.0f)).y));
        }
        else
        {
            lineRenderer.enabled = false;
        }
    }

    public void SetParabolaRender(bool enabled)
    {
        lineRenderer.enabled = enabled;
        this.enabled = enabled;
    }

    public override int GetHashCode()
    {
        return id;
    }

    public override bool Equals(object obj)
    {
        if (obj is Parabola)
        {
            Parabola other = obj as Parabola;
            return id == other.id;
        }
        return false;
    }
}
