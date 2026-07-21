using System;
using UnityEngine;


public class EnemyGoToPointState : State
{
    private EnemyController _enemy;
    private Func<Vector3> getDestination;   
    private float recalcInterval;
    private float lastMileDistance;
    private float recalcTimer;
    private Vector3 lastDestination;

    public EnemyGoToPointState(EnemyController enemy, Func<Vector3> getDestination,
                               float recalcInterval = 1f, float lastMileDistance = 4f)
    {
        _enemy = enemy;
        this.getDestination = getDestination;
        this.recalcInterval = recalcInterval;
        this.lastMileDistance = lastMileDistance;
    }

    public override void Enter()
    {
        lastDestination = getDestination();
        _enemy.CalculatePathTo(lastDestination);
        recalcTimer = 0f;
    }

    public override void Execute()
    {
        Vector3 destination = getDestination();
        Vector3 toDestination = (destination - _enemy.transform.position).NoY();

        recalcTimer += Time.deltaTime;
        if (recalcTimer >= recalcInterval)
        {
            // Solo recalculo si el destino se movió (si es fijo, evito oscilar entre nodos)
            if (Vector3.Distance(lastDestination, destination) > 1f)
            {
                lastDestination = destination;
                _enemy.CalculatePathTo(destination);
            }
            recalcTimer = 0f;
        }

        if (_enemy.FollowCurrentPath())
        {
            
            if (toDestination.magnitude < 0.5f)
            {
                _enemy.Stop();
                return;
            }

            _enemy.Move(toDestination);
        }
    }
    public override void Exit()
    {
        _enemy.Stop();
    }
}