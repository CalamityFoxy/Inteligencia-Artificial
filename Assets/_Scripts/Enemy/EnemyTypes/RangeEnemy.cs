using UnityEngine;

public class RangeEnemy : EnemyController
{
    QuestionNode rootNode;
    FSM rangeEnemyFsm;
    public Transform[] patrollWaypoints;
    public Transform shootPosition;
    public GameObject projectilePrefab;
    public float attackRange;
    public float attackCooldown;

    protected override void Awake()
    {
        base.Awake();
        rangeEnemyFsm = new FSM();
       // rangeEnemyFsm.RegisterState(EnemyStateType.Idle, new EnemyIdleState(this, homePoint));
        rangeEnemyFsm.RegisterState(EnemyStateType.Patroll, new RangeEnemy_PatrollState(this, patrollWaypoints));
        rangeEnemyFsm.RegisterState(EnemyStateType.Chase, new EnemyMelee_ChaseState(this, Target, attackRange, attackCooldown));
        rangeEnemyFsm.RegisterState(EnemyStateType.Attack, new RangeEnemy_AttackState(this, Target, attackRange, attackCooldown, shootPosition));
        rangeEnemyFsm.SetInitialState(EnemyStateType.Patroll);

        ActionNode respawning = new ActionNode(Respawn);
        ActionNode backToBase = new ActionNode(Respawn);
        ActionNode searchFlag = new ActionNode(SearchFlag);

        var patroll = new ActionNode(() => rangeEnemyFsm.SetState(EnemyStateType.Patroll));
        var chasePlayer = new ActionNode(() => rangeEnemyFsm.SetState(EnemyStateType.Chase));
        var attackPlayer = new ActionNode(() => rangeEnemyFsm.SetState(EnemyStateType.Attack));

        QuestionNode isFLagDropped = new QuestionNode(IsFlagDropped, searchFlag, chasePlayer);
        QuestionNode isFLagOnHome = new QuestionNode(IsFlagHome, patroll, isFLagDropped);
        QuestionNode isFLagOnMe = new QuestionNode(IsFlagOnMe, backToBase, isFLagOnHome);
        QuestionNode canAttack = new QuestionNode(TryAttack, attackPlayer, chasePlayer);
        QuestionNode canSeeTarget = new QuestionNode(IsTargetInLos, canAttack, isFLagOnMe);
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
