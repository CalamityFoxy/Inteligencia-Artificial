using System.Collections.Generic;
using UnityEngine;

public class AStarPathfinder : MonoBehaviour
{
    [SerializeField] private List<WaypointNode> allNodes;

    [Header("Autoconexión")]
    [SerializeField] private float maxConnectionDistance = 15f;  // distancia máx para conectar dos nodos
    [SerializeField] private LayerMask wallMask;                 
    [SerializeField] private float raycastHeight = 1f;           // altura del rayo para no rozar el piso x las dudas

    private void Awake()
    {
        AutoConnectNodes();
    }

    private void AutoConnectNodes()
    {
        foreach (var nodeA in allNodes)
        {
            nodeA.neighbours.Clear();   

            foreach (var nodeB in allNodes)
            {
                if (nodeA == nodeB) continue;   

               
                Vector3 posA = nodeA.transform.position + Vector3.up * raycastHeight;
                Vector3 posB = nodeB.transform.position + Vector3.up * raycastHeight;

                float distance = Vector3.Distance(posA, posB);

                
                if (distance > maxConnectionDistance) continue;

               
                Vector3 dir = (posB - posA).normalized;
                if (Physics.Raycast(posA, dir, distance, wallMask))
                    continue;

                
                nodeA.neighbours.Add(nodeB);
            }
        }
    }

    public WaypointNode GetClosestNode(Vector3 position)
    {
        WaypointNode closest = null;
        float closestDistance = Mathf.Infinity;
        foreach (var node in allNodes)
        {
            float distance = Vector3.Distance(position, node.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = node;
            }
        }
        return closest;
    }

    private void ResetNodes()
    {
        foreach (var node in allNodes)
        {
            node.gCost = Mathf.Infinity;
            node.hCost = 0;
            node.parent = null;
        }
    }

    public List<WaypointNode> FindPath(WaypointNode start, WaypointNode goal)
    {
        ResetNodes();   // limpiamos el estado de la búsqueda anterior al tener varios enemigos haciendo busqueda

        List<WaypointNode> openSet = new();
        HashSet<WaypointNode> closedSet = new();

        openSet.Add(start);
        start.gCost = 0;
        start.hCost = Vector3.Distance(start.transform.position, goal.transform.position);
        start.parent = null;

        while (openSet.Count > 0)
        {
            
            WaypointNode current = openSet[0];
            foreach (var node in openSet)
            {
                if (node.FCost < current.FCost)
                    current = node;
            }

           
            if (current == goal)
                return ReconstructPath(goal);

            openSet.Remove(current);
            closedSet.Add(current);

            foreach (WaypointNode neighbour in current.neighbours)
            {
                if (closedSet.Contains(neighbour))
                    continue;

               
                float tentativeG =
                    current.gCost +
                    Vector3.Distance(current.transform.position,
                                     neighbour.transform.position);

              
                if (!openSet.Contains(neighbour) || tentativeG < neighbour.gCost)
                {
                    neighbour.parent = current;
                    neighbour.gCost = tentativeG;
                    neighbour.hCost =
                        Vector3.Distance(neighbour.transform.position,
                                         goal.transform.position);

                    if (!openSet.Contains(neighbour))
                        openSet.Add(neighbour);
                }
            }
        }

      
        return null;
    }

    private List<WaypointNode> ReconstructPath(WaypointNode end)
    {
        List<WaypointNode> path = new();
        WaypointNode current = end;
        while (current != null)
        {
            path.Add(current);
            current = current.parent;
        }
        path.Reverse();
        return path;
    }
}