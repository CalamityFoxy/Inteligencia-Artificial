using System.Collections.Generic;
using UnityEngine;

public class WaypointNode : MonoBehaviour
{
    public List<WaypointNode> neighbours = new();

    [HideInInspector] public float gCost;
    [HideInInspector] public float hCost;
    [HideInInspector] public WaypointNode parent;

    public float FCost => gCost + hCost;

    // Dibuja el nodo y sus conexiones en la Scene view para verificar la red visualmente
    private void OnDrawGizmos()
    {
        // El nodo en sí (esfera cyan)
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(transform.position, 0.3f);

        // Líneas a cada vecino
        Gizmos.color = Color.yellow;
        foreach (var neighbour in neighbours)
        {
            if (neighbour != null)
                Gizmos.DrawLine(transform.position, neighbour.transform.position);
        }
    }
}