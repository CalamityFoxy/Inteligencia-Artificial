using System.Collections.Generic;
using UnityEngine;

public class EnemyMelee_PatrolState : State
{
    private EnemyController _enemy;
    private List<WaypointNode> waypoints;
    private int currentWaypoint = 0;
    private int direction = 1;
    private float waypointTolerance = 1.5f;

    private List<WaypointNode> currentPath;
    private int currentPathIndex;

    private AStarPathfinder pathfinder;

    private int patrolIterations = 0;
    private int iterationsBeforeRest;

    public bool ShouldRest => patrolIterations >= iterationsBeforeRest;

    public EnemyMelee_PatrolState(EnemyController enemy, List<WaypointNode> waypoints, AStarPathfinder pathfinder, int iterationsBeforeRest = 4)
    {
        this.pathfinder = pathfinder;
        _enemy = enemy;
        this.waypoints = waypoints;
        this.iterationsBeforeRest = iterationsBeforeRest;
    }

    public override void Enter()
    {
        patrolIterations = 0;
        CalculatePath();
    }

    private void CalculatePath()
    {
        WaypointNode start = pathfinder.GetClosestNode(_enemy.transform.position);

        WaypointNode end = waypoints[currentWaypoint];

        currentPath = pathfinder.FindPath(start, end);
        currentPathIndex = 0;
    }

    public override void Execute()
    {
        if (currentPath == null || currentPath.Count == 0)
            return;

        WaypointNode targetNode = currentPath[currentPathIndex];

        Vector3 dir = targetNode.transform.position - _enemy.transform.position;

        _enemy.Move(dir.NoY());

        if (dir.magnitude < waypointTolerance)
        {
            currentPathIndex++;

            if (currentPathIndex >= currentPath.Count)
            {
                AdvanceToNextWaypoint();
                CalculatePath();
            }
        }
    }

    private void AdvanceToNextWaypoint()
    {


        if (waypoints.Count == 1) return;

        currentWaypoint += direction;
        patrolIterations++;

       
        if (currentWaypoint >= waypoints.Count)
        {
            currentWaypoint = waypoints.Count - 2;
            direction = -1;
        }
        else if (currentWaypoint < 0)
        {
            currentWaypoint = 1;
            direction = 1;
        }

        
    }

    public override void Exit()
    {
        _enemy.Stop();
    }
}