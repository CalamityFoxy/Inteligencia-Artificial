using UnityEngine;

public class RangeEnemy_PatrollState : State
{
    private EnemyController _enemy;
    private Transform playerTarget;
    private Transform[] waypoints;

    private int currentWaypoint = 0;
    private float waypointTolerance = 0.6f;

    public RangeEnemy_PatrollState(EnemyController enemy, Transform target, Transform[] waypoints)
    {
        _enemy = enemy;
        playerTarget = target;
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
        _enemy.MoveWithSteering(dir);
        _enemy.Look(dir.NoY());

        // Chequear si llegó
        float distance = Vector3.Distance(_enemy.transform.position, waypoint.position);

        if (distance <= waypointTolerance)
        {
            currentWaypoint++;

            if (currentWaypoint >= waypoints.Length)
            {
                currentWaypoint = 0; // loop
            }
        }

        // detectar jugador y disparar
        float playerDistance = Vector3.Distance(_enemy.transform.position, playerTarget.position);

        if (playerDistance < 6f) // rango de detección
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        Debug.Log("Enemy shooting!");
        _enemy.Stop();
        _enemy.Look(playerTarget.position);
        // Ejemplo:
        // _enemy.Shoot(playerTarget);
    }

    public override void Exit()
    {
        _enemy.Stop();
    }
}