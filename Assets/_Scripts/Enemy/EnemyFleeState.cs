using UnityEngine;

public class EnemyFleeState : State
{
    private EnemyController _enemy;
    private Transform healingPoint;

    public EnemyFleeState(EnemyController enemy, Transform healingPoint)
    {
        _enemy = enemy;
        this.healingPoint = healingPoint;
    }

    public override void Enter()
    {
    }

    public override void Execute()
    {
        float distToPlayer = Vector3.Distance(_enemy.transform.position, _enemy.Target.position);

        if (distToPlayer < 5f)
        {
            // huye del jugador
            _enemy.FleeFromTarget();
        }
        else
        {
            // va a la zona de heal
            Vector3 dir = healingPoint.position - _enemy.transform.position;
            _enemy.MoveWithSteering(dir);
            _enemy.Look(dir);
        }
    }

    public override void Exit()
    {
        _enemy.Stop();
    }
}