
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
        Vector3 dir = _enemy.Target.position - _enemy.transform.position;
        dir.y = 0;
        _enemy.Move(dir.normalized);
        _enemy.Look(dir);
    }

    public override void Exit()
    {
        _enemy.Stop();
    }
}