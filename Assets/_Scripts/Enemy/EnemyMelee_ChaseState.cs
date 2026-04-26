using UnityEngine;

public class EnemyMelee_ChaseState : State
{
    private EnemyController _enemy;
    private Transform target;

    private float attackRange;
    private float attackCooldown;
    private float attackTimer;

    public EnemyMelee_ChaseState(EnemyController enemy, Transform target, float range, float cooldown)
    {
        _enemy = enemy;
        this.target = target;
        attackRange = range;
        attackCooldown = cooldown;
    }

    public override void Enter()
    {
        attackTimer = 0f;
    }

    public override void Execute()
    {
        attackTimer += Time.deltaTime;

        float distance = Vector3.Distance(target.position, _enemy.transform.position);

        // Dirección hacia el target
        Vector3 dir = target.position - _enemy.LastKnownTargetPosition;

        if (distance > attackRange)
        {
            _enemy.MoveWithSteering(dir);
            _enemy.Look(dir.NoY());
        }
        else
        {
            // DETENERSE PARA ATACAR
            _enemy.Stop();
            _enemy.Look(dir.NoY());

            // ATACAR CON COOLDOWN
            if (attackTimer >= attackCooldown)
            {
                Attack();
                attackTimer = 0f;
            }
        }
    }

    private void Attack()
    {
        Debug.Log("Enemy attacks!");


        // _enemy.Animator.SetTrigger("Attack");
    }

    public override void Exit()
    {
        _enemy.Stop();
    }
}