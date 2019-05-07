using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Site : MonoBehaviour
{

    public Material defaultMaterial;
    public Material fadedMaterial;
    public Material highlightMaterial;
    public bool isHighlighted;
    public bool isFaded;

    public Parabola mParabola;

    public int Degree { get; set; }

    static int siteCount = 0;

    new Renderer renderer;
    int id;

    void Awake()
    {
        id = siteCount;
        siteCount++;
    }

    // Use this for initialization
    void Start()
    {
        renderer = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isHighlighted)
        {
            renderer.material = highlightMaterial;
        }
        else if (isFaded)
        {
            renderer.material = fadedMaterial;
        }
        else
        {
            renderer.material = defaultMaterial;
        }
    }

    public override int GetHashCode()
    {
        return id;
    }

    public override bool Equals(object obj)
    {
        if (obj is Site)
        {
            Site other = obj as Site;
            return id == other.id;
        }
        return false;
    }
}
