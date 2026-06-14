using System.Collections.Generic;
using UnityEngine;

public class WaypointNode : MonoBehaviour
{
    public List<WaypointNode> neighbours = new();

    [HideInInspector] public float gCost;
    [HideInInspector] public float hCost;
    [HideInInspector] public WaypointNode parent;

    public float FCost => gCost + hCost;
}