
using UnityEngine;

public class EnemyPatrollState : State
{
    private EnemyController _enemy;
    private Transform playerTarget;
    private Transform homePoint;

    public EnemyPatrollState(EnemyController enemy, Transform target, Transform homepoint)
    {
        _enemy = enemy;
        playerTarget = target;
        homePoint = homepoint;
    }

    public override void Enter() { }

    public override void Execute()
    {
        Debug.Log("Patroll");
    }

    private void Shoot() { }

    public override void Exit()
    {
        _enemy.Stop();
    }
}