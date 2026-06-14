using UnityEngine;

public class EnemySearchState : State
{
    private EnemyController _enemy;
    private float recalcInterval;   // cada cuánto recalcula el path
    private float recalcTimer;

    public EnemySearchState(EnemyController enemy, float recalcInterval = 1f)
    {
        _enemy = enemy;
        this.recalcInterval = recalcInterval;
    }

    public override void Enter()
    {
        
        _enemy.CalculatePathTo(_enemy.LastKnownTargetPosition);
        recalcTimer = 0f;
    }

    public override void Execute()
    {
        recalcTimer += Time.deltaTime;

        // Recalculo cada X segundos 
        if (recalcTimer >= recalcInterval)
        {
            _enemy.CalculatePathTo(_enemy.LastKnownTargetPosition);
            recalcTimer = 0f;
        }

        // Sigo el camino nodo por nodo. Move ya aplica obstacle avoidance internamente.
        _enemy.FollowCurrentPath();
    }

    public override void Exit()
    {
        _enemy.Stop();
    }
}