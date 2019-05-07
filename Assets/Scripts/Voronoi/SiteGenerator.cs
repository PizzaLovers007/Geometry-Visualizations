using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SiteGenerator : MonoBehaviour
{
    public GameObject sitePrefab;
    public GameObject parabolaPrefab;

    public HashSet<Site> Sites { get; private set; }

    // Use this for initialization
    void Start()
    {
        Sites = new HashSet<Site>();
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector3 worldPos = new Vector3(ray.origin.x, ray.origin.y, 0);

        // Left click adds point
        if (Input.GetMouseButtonDown(0))
        {
            bool doAdd = true;
            RaycastHit[] hits = Physics.RaycastAll(ray);
            foreach (var hit in hits)
            {
                if (hit.transform.GetComponent<Site>() != null)
                {
                    doAdd = false;
                    break;
                }
            }
            if (doAdd)
            {
                //Vertex vert = Instantiate(vertexPrefab, worldPos, Quaternion.identity).GetComponent<Vertex>();
                Site site = Instantiate(sitePrefab, worldPos, Quaternion.identity).GetComponent<Site>();
                site.mParabola = Instantiate(parabolaPrefab, worldPos, Quaternion.identity).GetComponent<Parabola>();
                Sites.Add(site);
            }
        }
        // Right click removes point
        else if (Input.GetMouseButtonDown(1))
        {
            RaycastHit[] hits = Physics.RaycastAll(ray);
            foreach (var hit in hits)
            {
                if (hit.transform.GetComponent<Site>() != null)
                {
                    Site site = hit.transform.GetComponent<Site>();
                    Sites.Remove(site);
                    Destroy(site.mParabola.gameObject);
                    Destroy(hit.transform.gameObject);
                    break;
                }
            }
        }
    }
}
