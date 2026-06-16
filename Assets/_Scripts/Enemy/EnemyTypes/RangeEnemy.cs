using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class RangeEnemy : EnemyController
{
    QuestionNode rootNode;
    FSM rangeEnemyFsm;

    [Header("Patroll Settings")]
    public int iterationsBeforeRest = 4;
    public float idleDuration = 3f;

    [Header("Patrol")]
    public List<WaypointNode> patrolWaypoints;   // ? nuevo

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
        _patrolState = new EnemyMelee_PatrolState(this, patrolWaypoints, pathfinder, iterationsBeforeRest);  // ? patrolWaypoints

        rangeEnemyFsm.RegisterState(EnemyStateType.Idle, _idleState);
        rangeEnemyFsm.RegisterState(EnemyStateType.Patroll, _patrolState);
        rangeEnemyFsm.RegisterState(EnemyStateType.Chase, new EnemyRange_ChaseState(this, Target, attackRange, attackCooldown));
        rangeEnemyFsm.RegisterState(EnemyStateType.Attack, new RangeEnemy_AttackState(this, Target, attackRange, attackCooldown, shootPosition));
        rangeEnemyFsm.RegisterState(EnemyStateType.Search, new EnemySearchState(this));   // ? reusamos el Search del melee
      //  rangeEnemyFsm.SetInitialState(EnemyStateType.Patroll);

        // Action nodes
        ActionNode respawning = new ActionNode(Respawn);
        var patroll = new ActionNode(() => rangeEnemyFsm.SetState(EnemyStateType.Patroll));
        var chasePlayer = new ActionNode(() => rangeEnemyFsm.SetState(EnemyStateType.Chase));
        var attackPlayer = new ActionNode(() => rangeEnemyFsm.SetState(EnemyStateType.Attack));
        var idle = new ActionNode(() => rangeEnemyFsm.SetState(EnemyStateType.Idle));
        var search = new ActionNode(() => rangeEnemyFsm.SetState(EnemyStateType.Search));

        
        QuestionNode idleOrPatrol = new QuestionNode(IdleFinished, patroll, idle);
        QuestionNode notSeeingPlayer = new QuestionNode(PatrolNeedsRest, idleOrPatrol, patroll);

        
        QuestionNode canAttack = new QuestionNode(TryAttack, attackPlayer, chasePlayer);
        // le agregamos el Search al rango
        QuestionNode seeOrSearch = new QuestionNode(() => CanSeeTarget, canAttack, search);
        
        QuestionNode canSeeTarget = new QuestionNode(IsTargetTracked, seeOrSearch, notSeeingPlayer);

        QuestionNode isAlive = new QuestionNode(IsAlive, canSeeTarget, respawning);

        rootNode = isAlive;
    }
    protected virtual void Start()
    {
        rangeEnemyFsm.SetInitialState(EnemyStateType.Patroll);
    }

    private bool TryAttack() // Verifica si el enemigo puede atacar al jugador, es decir, si el jugador esta dentro del rango de ataque y el enemigo lo puede ver
    {
        var distance = Vector3.Distance(Target.position, transform.position);
        if (IsTargetInLos() && distance <= attackRange) {  return true; } else return false;
    }

    public void ShootProjectile(Transform shootPoint, Vector3 targetPosition) // Metodo unico del RangeEnemy para disparar un proyectil hacia el jugador
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
        Debug.Log(rangeEnemyFsm.CurrentState);
    }

    public bool IdleFinished() => _idleState != null && _idleState.IdleFinished; 
    public bool PatrolNeedsRest() => _patrolState != null && _patrolState.ShouldRest;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
