
using UnityEngine;

public class EnemyChaseState : State
{
    private EnemyController _enemy;

    public EnemyChaseState(EnemyController enemy)
    {
        _enemy = enemy;
    }

    public override void Enter() { }

    public override void Execute()
    {
        Vector3 dir = _enemy.LastKnownTargetPosition - _enemy.transform.position;
        _enemy.MoveWithSteering(dir);
        _enemy.Look(dir.NoY());
    }

    public override void Exit()
    {
        _enemy.Stop();
    }
}