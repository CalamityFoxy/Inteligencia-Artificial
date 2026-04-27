using UnityEngine;

public class EnemyMelee_PatrolState : State
{
    private EnemyController _enemy;
    [SerializeField] private Transform[] waypoints;
    private int currentWaypoint = 0;
    private int direction = 1;         
    private float waypointTolerance = 1.5f;

    
    private int patrolIterations = 0;
    private int iterationsBeforeRest;

    public bool ShouldRest => patrolIterations >= iterationsBeforeRest;

    public EnemyMelee_PatrolState(EnemyController enemy, Transform[] waypoints, int iterationsBeforeRest = 4)
    {
        _enemy = enemy;
        this.waypoints = waypoints;
        this.iterationsBeforeRest = iterationsBeforeRest;
    }

    public override void Enter()
    {
       
        patrolIterations = 0;
    }

    public override void Execute()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Transform waypoint = waypoints[currentWaypoint];
        Vector3 dir = (waypoint.position - _enemy.transform.position).NoY();
        float distance = dir.magnitude;

       

        _enemy.Move(dir);
        

        if (distance <= waypointTolerance)
        {
            AdvanceToNextWaypoint();
        }
    }

    private void AdvanceToNextWaypoint()
    {


        if (waypoints.Length == 1) return;

        currentWaypoint += direction;
        patrolIterations++;

       
        if (currentWaypoint >= waypoints.Length)
        {
            currentWaypoint = waypoints.Length - 2;
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