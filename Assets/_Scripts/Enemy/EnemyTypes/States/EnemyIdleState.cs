using UnityEngine;

public class EnemyIdleState : State
{
    private EnemyController _enemy;
    private float idleDuration;
    private float idleTimer;
    private Vector3 idleLookDirection;

    public bool IdleFinished => idleTimer >= idleDuration;

    public EnemyIdleState(EnemyController enemy, float idleDuration = 2.5f)
    {
        _enemy = enemy;
        this.idleDuration = idleDuration;
    }

    public override void Enter()
    {
        idleTimer = 0f;
        _enemy.Stop();

        idleLookDirection = _enemy.transform.forward;
    }

    public override void Execute()
    {
        idleTimer += Time.deltaTime;
        
      
        _enemy.Stop();
        _enemy.Look(idleLookDirection);
    }

    public override void Exit()
    {
        idleTimer = 0f;
    }
}