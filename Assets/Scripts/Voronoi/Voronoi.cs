using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Voronoi : MonoBehaviour {

    SiteGenerator siteGenerator;
    public Material defaultMaterial;
    public GameObject voronoiEdgePrefab;
    private const int parabolaSegments = 100;
    private static bool renderParabolas = true;
    HashSet<VoronoiEdge> edges;


    float mouseYpos = -9999.0f;
    List<Site> sites;
    SortedSet<VoronoiEvent> eventQueue;
    VoronoiNode root = null;
    HashSet<VoronoiEvent> removed = new HashSet<VoronoiEvent>();
    float sweepY;


    // Use this for initialization
    void Start () {
        sites = new List<Site>();
        edges = new HashSet<VoronoiEdge>();
        siteGenerator = GameObject.Find("SiteGenerator").GetComponent<SiteGenerator>();
    }

    void Update ()
    {
        
        
        List<Site> sites = new List<Site>(siteGenerator.Sites);
        float newMouseYpos = Camera.main.ScreenToWorldPoint(Input.mousePosition).y;
        if (Input.GetKeyDown(KeyCode.G))
        {
            renderParabolas = !renderParabolas;
            foreach (Site site in sites)
            {
                site.mParabola.SetParabolaRender(renderParabolas);
            }

        }

        if (sites.Count != 0 && renderParabolas && newMouseYpos != mouseYpos)
        {
            root = null;
            foreach (VoronoiEdge oldEdge in edges)
            {
                Destroy(oldEdge.gameObject);
            }
            edges.Clear();
            mouseYpos = newMouseYpos;
            foreach (Site site in sites)
            {
                site.mParabola.CalculateParabola(site, newMouseYpos);
            }
            eventQueue = new SortedSet<VoronoiEvent>(new EventComparer());
            foreach (Site site in sites)
            {
                if(site.transform.position.y <= mouseYpos)
                {
                    eventQueue.Add(new SiteEvent(site.transform.position));
                }
            }
            while(eventQueue.Count != 0)
            {
                VoronoiEvent popEvent = eventQueue.Min;
                eventQueue.Remove(popEvent);
                sweepY = popEvent.yCoord;
                if(removed.Contains(popEvent))
                {
                    removed.Remove(popEvent);
                    continue;
                }
                if(popEvent.GetType() == typeof(SiteEvent))
                {
                    AddArc(((SiteEvent)popEvent).mSite);
                }
                else
                {
                    RemoveArc((SqueezeEvent)popEvent);
                }
            }
            if(root != null)
            {
                CompleteEdge(root);

            }
        }



    }

    public void AddArc(Vector2 newSite)
    {
        if(root == null)
        {
            root = new VoronoiArcNode(newSite);
        }
        else
        {
            VoronoiArcNode newArc = new VoronoiArcNode(newSite);

            VoronoiArcNode underNew = GetParabolaUnderNew(newArc.mFocus.x);

            if(underNew.squeezeEvent != null)
            {
                removed.Add(underNew.squeezeEvent);
                underNew.squeezeEvent = null;
            }

            Vector2 startPoint = new Vector2(newSite.x, GetYCoord(underNew.mFocus, newSite.x));

            VoronoiArcNode leftArc = new VoronoiArcNode(underNew.mFocus);
            VoronoiArcNode rightArc = new VoronoiArcNode(underNew.mFocus);
            VoronoiEdgeNode rightEdge = new VoronoiEdgeNode(startPoint, new Vector2(), underNew.mFocus, newSite);
            VoronoiEdgeNode leftEdge = new VoronoiEdgeNode(startPoint, new Vector2(), newSite, underNew.mFocus);

            leftEdge.pair = rightEdge;
            //*******
            //edges.Add(leftEdge);
            newArc.mParent = rightEdge;

            rightEdge.mLeft = newArc;
            rightEdge.mRight = rightArc;
            rightEdge.mParent = leftEdge;

            leftEdge.mLeft = leftArc;
            leftEdge.mRight = rightEdge;
            leftEdge.mParent = underNew.mParent;

            leftArc.mParent = leftEdge;
            rightArc.mParent = rightEdge;

            if(underNew == root)
            {
                root = leftEdge;
            }
            else if(underNew.mParent.mLeft == underNew)
            {
                underNew.mParent.mLeft = leftEdge;
            }
            else if(underNew.mParent.mRight == underNew)
            {
                underNew.mParent.mRight = leftEdge;
            }
            else
            {
                Debug.Log("Invalid Tree");
            }

            SqueezeEvent(leftArc);
            SqueezeEvent(rightArc);
        }
    }

    public void RemoveArc(SqueezeEvent vEvent)
    {
        VoronoiArcNode removeNode = vEvent.mArcToRemove;

        VoronoiEdgeNode leftEdge = VoronoiNode.GetLeftParentEdge(removeNode);
        VoronoiEdgeNode rightEdge = VoronoiNode.GetRightParentEdge(removeNode);

        VoronoiArcNode leftArc = VoronoiNode.GetLeftChildArc(leftEdge);
        VoronoiArcNode rightArc = VoronoiNode.GetRightChildArc(rightEdge);

        if(leftArc.squeezeEvent != null)
        {
            removed.Add(leftArc.squeezeEvent);
            leftArc.squeezeEvent = null;
        }
        if(rightArc.squeezeEvent != null)
        {
            removed.Add(rightArc.squeezeEvent);
            rightArc.squeezeEvent = null;
        }

        Vector2 point = new Vector2(vEvent.mIntersectionPoint.x, GetYCoord(removeNode.mFocus, vEvent.mIntersectionPoint.x));

        leftEdge.mEndVertex = point;
        rightEdge.mEndVertex = point;

        VoronoiNode h;
        VoronoiNode tempNode = removeNode;
        while(tempNode != root)
        {
            tempNode = tempNode.mParent;
            if(tempNode == leftEdge)
            {
                h = leftEdge;
            }
            if(tempNode == rightEdge)
            {
                h = rightEdge;
            }
        }
        h = new VoronoiEdgeNode(point, new Vector2(), leftArc.mFocus, rightArc.mFocus); //*******
        //edges.Add((VoronoiEdgeNode)h);
        


        VoronoiNode removeNodeGrandparent = removeNode.mParent.mParent;
        if(removeNode.mParent.mLeft == removeNode)
        {
            if(removeNodeGrandparent.mLeft == removeNode.mParent)
            {
                removeNodeGrandparent.mLeft = removeNode.mParent.mRight;
                removeNode.mParent.mRight.mParent = removeNodeGrandparent;
            }
            if(removeNodeGrandparent.mRight == removeNode.mParent)
            {
                removeNodeGrandparent.mRight = removeNode.mParent.mRight;
                removeNode.mParent.mRight.mParent = removeNodeGrandparent;
            }
        }
        else
        {
            if (removeNodeGrandparent.mLeft == removeNode.mParent)
            {
                removeNodeGrandparent.mLeft = removeNode.mParent.mLeft;
                removeNode.mParent.mLeft.mParent = removeNodeGrandparent;
            }
            if (removeNodeGrandparent.mRight == removeNode.mParent)
            {
                removeNodeGrandparent.mRight = removeNode.mParent.mLeft;
                removeNode.mParent.mLeft.mParent = removeNodeGrandparent;
            }
        }

        

        SqueezeEvent(leftArc);
        SqueezeEvent(rightArc);

    }

    public VoronoiArcNode GetParabolaUnderNew(float newSiteX)
    {
        VoronoiNode checkNode = root;
        float xCoord = 0.0f;

        while(checkNode.hasChildren())
        {
            xCoord = GetXCoord(checkNode, sweepY);
            if(xCoord > newSiteX)
            {
                checkNode = checkNode.mLeft;
            }
            else
            {
                checkNode = checkNode.mRight;
            }
        }

        if(checkNode.GetType() == typeof(VoronoiArcNode))
        {
            return (VoronoiArcNode)checkNode;
        }
        else
        {
            Debug.Log("Failed to get ArcNode on GetParabolaUnderNew");
            return null;
        }
    }

    public float GetXCoord(VoronoiNode node, float yPos)
    {
        VoronoiArcNode left = VoronoiNode.GetLeftChildArc(node);
        VoronoiArcNode right = VoronoiNode.GetRightChildArc(node);

        Vector2 leftFocus = left.mFocus;
        Vector2 rightFocus = right.mFocus;

        // Get the x coordinate intersections by using the discriminant
        // Work backwards from the parabola equation to get the a, b, and c values
        float dp = 2.0f * (leftFocus.y - yPos);
        float a1 = 1.0f / dp;
        float b1 = -2.0f * leftFocus.x / dp;
        float c1 = yPos + dp / 4.0f + leftFocus.x * leftFocus.x / dp;

        dp = 2.0f * (rightFocus.y - yPos);
        float a2 = 1.0f / dp;
        float b2 = -2.0f * rightFocus.x / dp;
        float c2 = yPos + dp / 4.0f + rightFocus.x * rightFocus.x / dp;

        float a = a1 - a2;
        float b = b1 - b2;
        float c = c1 - c2;

        float discriminant = b * b - 4 * a * c;
        float x1 = (-b + Mathf.Sqrt(discriminant)) / (2 * a);
        float x2 = (-b - Mathf.Sqrt(discriminant)) / (2 * a);
        

        float xCoord;
        if (leftFocus.y < rightFocus.y)
        {
            xCoord = Mathf.Max(x1, x2);
        }
        else
        {
            xCoord = Mathf.Min(x1, x2);
        }

        return xCoord;
    }

    public float GetYCoord(Vector2 focus, float xPos)
    {
        float dp = 2 * (focus.y - sweepY);
        float a1 = 1 / dp;
        float b1 = -2 * focus.x / dp;
        float c1 = sweepY + dp / 4 + focus.x * focus.x / dp;

        return (a1 * xPos * xPos + b1 * xPos + c1);
    }

    public Vector2 GetIntersection(VoronoiEdgeNode a, VoronoiEdgeNode b)
    {
        float xIntersectionPoint = (b.yIntercept - a.yIntercept) / (a.slope - b.slope);
        float yIntersectionPoint = a.slope * xIntersectionPoint + a.yIntercept;

        if((xIntersectionPoint - a.mStartVertex.x)/a.mDirection.x < 0)
        {
            return new Vector2(-10000, -10000);
        }
        if ((yIntersectionPoint - a.mStartVertex.y) / a.mDirection.y < 0)
        {
            return new Vector2(-10000, -10000);
        }

        if ((xIntersectionPoint - b.mStartVertex.x) / b.mDirection.x < 0)
        {
            return new Vector2(-10000, -10000);
        }
        if ((yIntersectionPoint - b.mStartVertex.y) / b.mDirection.y < 0)
        {
            return new Vector2(-10000, -10000);
        }

        return new Vector2(xIntersectionPoint, yIntersectionPoint);
    }


    public void SqueezeEvent(VoronoiArcNode node)
    {
        VoronoiEdgeNode leftEdge = VoronoiNode.GetLeftParentEdge(node);
        VoronoiEdgeNode rightEdge = VoronoiNode.GetRightParentEdge(node);

        VoronoiArcNode leftArc = VoronoiNode.GetLeftChildArc(leftEdge);
        VoronoiArcNode rightArc = VoronoiNode.GetRightChildArc(rightEdge);

        if(leftArc == null || rightArc == null || leftArc.mFocus == rightArc.mFocus)
        {
            return;
        }

        Vector2 intersection = GetIntersection(leftEdge, rightEdge);
        if(intersection.Equals(new Vector2(-10000,-10000)))
        {
            return;
        }

        float circleX = leftArc.mFocus.x - intersection.x;
        float circleY = leftArc.mFocus.y - intersection.y;
        float radius = Mathf.Sqrt((circleX * circleX) + (circleY * circleY));

        if(intersection.y - radius >= sweepY)
        {
            return;
        }


        SqueezeEvent squeezeEvent = new SqueezeEvent(intersection, node);
        node.squeezeEvent = squeezeEvent;
        eventQueue.Add(squeezeEvent);
    }

    public void CompleteEdge(VoronoiNode node)
    {
        if(!node.hasChildren())
        {
            return;
        }
        VoronoiEdgeNode edge = (VoronoiEdgeNode)node;
        float height = Camera.main.orthographicSize;
        float width = height * Camera.main.aspect;
        float max;
        if(edge.mDirection.x > 0.0f)
        {
            max = Mathf.Max(width, edge.mStartVertex.x);
        }
        else
        {
            max = Mathf.Min(-width, edge.mStartVertex.x);
        }

        Vector2 endVertex = new Vector2(max, max * edge.slope + edge.yIntercept);
        edge.mEndVertex = endVertex;

        VoronoiEdge newRenderEdge = Instantiate(voronoiEdgePrefab, new Vector2(), Quaternion.identity).GetComponent<VoronoiEdge>();
        newRenderEdge.SetPosition(edge.mStartVertex, edge.mEndVertex);
        edges.Add(newRenderEdge);

        CompleteEdge(node.mLeft);
        CompleteEdge(node.mRight);
    }
}


class EventComparer : IComparer<VoronoiEvent>
{
    public int Compare(VoronoiEvent e1, VoronoiEvent e2)
    {
        return e1.yCoord.CompareTo(e2.yCoord);
    }
}