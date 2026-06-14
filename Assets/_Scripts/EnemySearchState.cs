using UnityEngine;

public class EnemySearchState : State
{
    private EnemyController _enemy;
    private float recalcInterval;
    private float recalcTimer;
    private Vector3 lastDestination;   // ? guardo el último destino calculado

    public EnemySearchState(EnemyController enemy, float recalcInterval = 1f)
    {
        _enemy = enemy;
        this.recalcInterval = recalcInterval;
    }

    public override void Enter()
    {
        lastDestination = _enemy.LastKnownTargetPosition;
        _enemy.CalculatePathTo(lastDestination);
        recalcTimer = 0f;
    }

    public override void Execute()
    {
        recalcTimer += Time.deltaTime;

        if (recalcTimer >= recalcInterval)
        {
            // Solo recalculo si el destino cambió de forma significativa
            if (Vector3.Distance(lastDestination, _enemy.LastKnownTargetPosition) > 1f)
            {
                lastDestination = _enemy.LastKnownTargetPosition;
                _enemy.CalculatePathTo(lastDestination);
            }
            recalcTimer = 0f;
        }

        _enemy.FollowCurrentPath();
    }

    public override void Exit()
    {
        _enemy.Stop();
    }
}