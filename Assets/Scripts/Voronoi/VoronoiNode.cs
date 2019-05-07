using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoronoiNode {

    public VoronoiNode mParent;
    public VoronoiNode mLeft;
    public VoronoiNode mRight;

    public VoronoiNode()
    {
        mParent = null;
        mLeft = null;
        mRight = null;
    }

    public void setLeftNode(VoronoiNode left)
    {
        left.mParent = this;
        mLeft = left;
    }

    public void setRightNode(VoronoiNode right)
    {
        right.mParent = this;
        mRight = right;
    }

    public void setParent(VoronoiNode node)
    {
        if (node.mParent == null)
        {
            mParent = null;
            return;
        }

        if (node.mParent.mLeft == node)
        {
            node.mParent.setLeftNode(this);
        }
        else
        {
            node.mParent.setRightNode(this);
        }
    }

    public static VoronoiArcNode GetLeftChildArc(VoronoiNode node)
    {
        if (node == null)
        {
            return null;
        }
        VoronoiNode returnNode = node.mLeft;
        while(returnNode.hasChildren())
        {
            returnNode = returnNode.mRight;
        }
        if(returnNode.GetType() == typeof(VoronoiArcNode))
        {
            return (VoronoiArcNode)returnNode;
        }
        Debug.Log("Failed to get ArcNode from GetLeftChildArc");
        return null;
    }

    public static VoronoiArcNode GetRightChildArc(VoronoiNode node)
    {
        if(node == null)
        {
            return null;
        }
        VoronoiNode returnNode = node.mRight;
        while (returnNode.hasChildren())
        {
            returnNode = returnNode.mLeft;
        }
        if (returnNode.GetType() == typeof(VoronoiArcNode))
        {
            return (VoronoiArcNode)returnNode;
        }
        else
        {
            Debug.Log("Failed to get ArcNode from GetRightChildArc");
            return null;
        }
    }

    public static VoronoiEdgeNode GetLeftParentEdge(VoronoiNode node)
    {
        VoronoiNode returnNode = node.mParent;
        VoronoiNode travelledNode = node;
        if(returnNode == null)
        {
            Debug.Log("null");
        }
        while(returnNode.mLeft == travelledNode)
        {
            if(returnNode.mParent == null)
            {
                return null;
            }
            travelledNode = returnNode;
            returnNode = returnNode.mParent;
        }
        if (returnNode.GetType() == typeof(VoronoiEdgeNode))
        {
            return (VoronoiEdgeNode)returnNode;
        }
        else
        {
            Debug.Log("Failed to get EdgeNode from GetLeftParentArc");
            return null;
        }
    }

    public static VoronoiEdgeNode GetRightParentEdge(VoronoiNode node)
    {
        VoronoiNode returnNode = node.mParent;
        VoronoiNode travelledNode = node;
        while (returnNode.mRight == travelledNode)
        {
            if (returnNode.mParent == null)
            {
                return null;
            }
            travelledNode = returnNode;
            returnNode = returnNode.mParent;
        }
        if (returnNode.GetType() == typeof(VoronoiEdgeNode))
        {
            return (VoronoiEdgeNode)returnNode;
        }
        else
        {
            Debug.Log("Failed to get EdgeNode from GetRightParentArc");
            return null;
        }
    }

    public bool hasChildren()
    {
        if(mLeft != null || mRight != null)
        {
            return true;
        }
        return false;
    }
}

public class VoronoiEdgeNode : VoronoiNode
{
    public Vector2 mStartVertex;
    public Vector2 mEndVertex;
    public Vector2 mDirection;
    public Vector2 mLeftSite;
    public Vector2 mRightSite;
    public VoronoiEdgeNode pair;
    public float slope;
    public float yIntercept;

    public VoronoiEdgeNode(Vector2 startVertex, Vector2 endVertex, Vector2 leftSite, Vector2 rightSite)
    {
        mStartVertex = startVertex;
        mEndVertex = endVertex;
        mLeftSite = leftSite;
        mRightSite = rightSite;
        pair = null;
        slope = (rightSite.x - leftSite.x) / (leftSite.y - rightSite.y);
        yIntercept = mStartVertex.y - slope * mStartVertex.x;
        mDirection = new Vector2(rightSite.y - leftSite.y, -(rightSite.x - leftSite.x));
    }

}

public class VoronoiArcNode : VoronoiNode
{
    public Vector2 mFocus;
    public VoronoiEvent squeezeEvent;
    public VoronoiArcNode(Vector2 focus)
    {
        mFocus = focus;
        squeezeEvent = null;
    }
}
