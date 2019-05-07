using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoronoiEvent {
    public float yCoord;
}

public class SiteEvent : VoronoiEvent
{
    public Vector2 mSite;
    public SiteEvent(Vector2 site)
    {
        mSite = site;
        yCoord = mSite.y;
    }
}

public class SqueezeEvent : VoronoiEvent
{
    public Vector2 mIntersectionPoint;
    public VoronoiArcNode mArcToRemove;
    public SqueezeEvent(Vector2 intersectionPoint, VoronoiArcNode arcToRemove)
    {
        mIntersectionPoint = intersectionPoint;
        mArcToRemove = arcToRemove;
        yCoord = mIntersectionPoint.y;
    }
}