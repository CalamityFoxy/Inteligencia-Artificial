using UnityEngine;

public class RangeEnemy_PatrollState : State
{
    private EnemyController _enemy;
    private Transform[] waypoints;

    private int currentWaypoint = 0;
    private float waypointTolerance = 0.6f;

    public RangeEnemy_PatrollState(EnemyController enemy, Transform[] waypoints)
    {
        _enemy = enemy;
        this.waypoints = waypoints; 
    }

    public override void Enter()
    {
        currentWaypoint = 0;
    }

    public override void Execute()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Transform waypoint = waypoints[currentWaypoint];

        Vector3 dir = waypoint.position - _enemy.transform.position;

        // Moverse hacia el waypoint
        _enemy.Move(dir.NoY());

        // Chequear si llegó
        float distance = Vector3.Distance(_enemy.transform.position, waypoint.position);

        if (distance <= waypointTolerance)
        {
            currentWaypoint++;

            if (currentWaypoint >= waypoints.Length)
            {
                currentWaypoint = 0; 
            }
        }
    }

    public override void Exit()
    {
        _enemy.Stop();
    }
}

