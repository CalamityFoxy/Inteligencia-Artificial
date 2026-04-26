
using UnityEngine;

public class EnemyIdleState : State
{
    private EnemyController _enemy;
    private Transform homePoint;

    public EnemyIdleState(EnemyController enemy, Transform homepoint)
    {
        _enemy = enemy;
        homePoint = homepoint;
    }

    public override void Enter()
    {
        
    }

    public override void Execute()
    {
        Vector3 dir = _enemy.homePoint.position - _enemy.transform.position;
        
        if (dir.magnitude > 0.5f)
        {
            _enemy.Move(dir.normalized);
            _enemy.Look(dir);
        }
        else
        {
            _enemy.Stop();
            _enemy.Look(_enemy.homePoint.forward);
        }
    }

    public override void Exit() { }
}