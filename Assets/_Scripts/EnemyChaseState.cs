
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
        _enemy.MoveWithSteering(dir.normalized);
        _enemy.Look(dir);
    }

    public override void Exit()
    {
        _enemy.Stop();
    }
}public class EnemyPatrollState : State
{
    private EnemyController _enemy;

    public EnemyPatrollState(EnemyController enemy)
    {
        _enemy = enemy;
    }

    public override void Enter() { }

    public override void Execute()
    {
        Debug.Log("Patroll");
    }

    public override void Exit()
    {
        _enemy.Stop();
    }
}