using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class RangeEnemy : EnemyController
{
    QuestionNode rootNode;
    FSM rangeEnemyFsm;
    [Header("Patroll Settings")]
    public Transform[] patrollWaypoints;
    public int iterationsBeforeRest = 4;
    public float idleDuration = 3f;

    [Header("Range Attack Properties")]
    public Transform shootPosition;
    public GameObject projectilePrefab;
    public float attackRange;
    public float attackCooldown;

    private EnemyIdleState _idleState;
    private EnemyMelee_PatrolState _patrolState;

    protected override void Awake()
    {
        base.Awake();
        rangeEnemyFsm = new FSM();

        _idleState = new EnemyIdleState(this, idleDuration);
        _patrolState = new EnemyMelee_PatrolState(this, patrollWaypoints, iterationsBeforeRest);

        rangeEnemyFsm.RegisterState(EnemyStateType.Idle, _idleState);
        rangeEnemyFsm.RegisterState(EnemyStateType.Patroll, _patrolState);
        rangeEnemyFsm.RegisterState(EnemyStateType.Chase, new EnemyMelee_ChaseState(this, Target, attackRange, attackCooldown));
        rangeEnemyFsm.RegisterState(EnemyStateType.Attack, new RangeEnemy_AttackState(this, Target, attackRange, attackCooldown, shootPosition));
        rangeEnemyFsm.SetInitialState(EnemyStateType.Patroll);

        ActionNode respawning = new ActionNode(Respawn);
        ActionNode backToBase = new ActionNode(Respawn);
        ActionNode searchFlag = new ActionNode(SearchFlag);

        var patroll = new ActionNode(() => rangeEnemyFsm.SetState(EnemyStateType.Patroll));
        var chasePlayer = new ActionNode(() => rangeEnemyFsm.SetState(EnemyStateType.Chase));
        var attackPlayer = new ActionNode(() => rangeEnemyFsm.SetState(EnemyStateType.Attack));
        var idle = new ActionNode(() => rangeEnemyFsm.SetState(EnemyStateType.Idle));

        
        QuestionNode isFLagDropped = new QuestionNode(IsFlagDropped, searchFlag, chasePlayer);
        QuestionNode isFLagOnHome = new QuestionNode(IsFlagHome, patroll, isFLagDropped);
        QuestionNode isFLagOnMe = new QuestionNode(IsFlagOnMe, backToBase, isFLagOnHome);

        QuestionNode idleOrPatrol = new QuestionNode(IdleFinished, patroll, idle);
        QuestionNode notSeeingPlayer = new QuestionNode(PatrolNeedsRest, idleOrPatrol, patroll);

        QuestionNode canAttack = new QuestionNode(TryAttack, attackPlayer, chasePlayer);
        QuestionNode canSeeTarget = new QuestionNode(IsTargetInLos, canAttack, notSeeingPlayer);
        QuestionNode isAlive = new QuestionNode(IsAlive, canSeeTarget, respawning);

        rootNode = isAlive;
    }

    private bool TryAttack()
    {
        var distance = Vector3.Distance(Target.position, transform.position);
        if (IsTargetInLos() && distance <= attackRange) {  return true; } else return false;
    }


    public void ShootProjectile(Transform shootPoint, Vector3 targetPosition)
    {
        Vector3 dir = (targetPosition - shootPoint.position).normalized;

        GameObject proj = Instantiate(projectilePrefab, shootPoint.position, Quaternion.LookRotation(dir));

        Projectile p = proj.GetComponent<Projectile>();
        if (p != null)
        {
            p.Init(dir);
        }
    }

    protected override void Update()
    {
        base.Update();
        rootNode.Execute();
        rangeEnemyFsm.Execute();
        // Debug.Log(rangeEnemyFsm.CurrentState);
    }


    public bool IdleFinished() => _idleState != null && _idleState.IdleFinished;
    public bool PatrolNeedsRest() => _patrolState != null && _patrolState.ShouldRest;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
