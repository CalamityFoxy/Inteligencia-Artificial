using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class RangeEnemy_AttackState : State
{
    private RangeEnemy _enemy;
    private Transform playerTarget;
    private float attackRange;
    private float attackCooldown;
    private float attackTimer;
    private Transform shootPosition;
    private GameObject bulletPrefab;

    public RangeEnemy_AttackState(RangeEnemy enemy, Transform target, float AttackRange, float AttackCooldown, Transform shootPosition)
    {
        _enemy = enemy;
        playerTarget = target;
        attackRange = AttackRange;
        attackCooldown = AttackCooldown;
        this.shootPosition = shootPosition;
    }

    public override void Enter()
    {
    }

    public override void Execute()
    {
        attackTimer += Time.deltaTime;
        Vector3 dir = playerTarget.position - _enemy.transform.position;

        _enemy.Look(dir.NoY());
        _enemy.Stop();
        float playerDistance = Vector3.Distance(_enemy.transform.position, playerTarget.position);

        if (playerDistance < attackRange && attackTimer >= attackCooldown) 
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        attackTimer = 0f;
        _enemy.ShootProjectile(shootPosition, playerTarget.position);
    }

    public override void Exit()
    {
        _enemy.Stop();
    }
}

