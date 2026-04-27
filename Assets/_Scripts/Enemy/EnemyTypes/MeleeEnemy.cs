



using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class MeleeEnemy : EnemyController
{
    QuestionNode rootNode;
    FSM meleeEnemyFsm;

    public float meleeMaxHealth = 200f;
    public float attackRange = 2f;
    public float attackCooldown = 3f;

    public Transform[] patrolWaypoints;
    public int iterationsBeforeRest = 4;
    public float idleDuration = 3f;
    private EnemyIdleState _idleState;
    private EnemyMelee_PatrolState _patrolState;
    protected override void Awake()
    {

        maxHealth = meleeMaxHealth;
        base.Awake();

        meleeEnemyFsm = new FSM();


        _idleState = new EnemyIdleState(this, idleDuration);// estos los creo para que el behaviour Tree los guarde de referencia y los cambie despues y asi la FSM no se entera de lo que esta pasando dentro del estado ni sus metodos(ni deberia).
        _patrolState = new EnemyMelee_PatrolState(this, patrolWaypoints, iterationsBeforeRest);// lo mismo acá

        meleeEnemyFsm.RegisterState(EnemyStateType.Idle, _idleState);
        meleeEnemyFsm.RegisterState(EnemyStateType.Patroll, _patrolState);
        meleeEnemyFsm.RegisterState(EnemyStateType.Chase, new EnemyMelee_ChaseState(this, Target, attackRange, attackCooldown));


        ActionNode respawning = new ActionNode(Respawn);
        ActionNode searchFlag = new ActionNode(SearchFlag);
        ActionNode attackPlayer = new ActionNode(AttackPlayer);
        ActionNode goBase = new ActionNode(returnToBase);

        var patrol = new ActionNode(() => meleeEnemyFsm.SetState(EnemyStateType.Patroll));
        var idle = new ActionNode(() => meleeEnemyFsm.SetState(EnemyStateType.Idle));
        var chase = new ActionNode(() => meleeEnemyFsm.SetState(EnemyStateType.Chase));

        meleeEnemyFsm.SetInitialState(EnemyStateType.Patroll);

        //QuestionNode isFlagOnMe = new QuestionNode(IsFlagOnMe,goBase,)
        QuestionNode idleOrPatrol = new QuestionNode(IdleFinished, patrol, idle);
        QuestionNode notSeeingPlayer = new QuestionNode(PatrolNeedsRest, idleOrPatrol, patrol);
        QuestionNode canSee = new QuestionNode(IsTargetTracked, chase, notSeeingPlayer);
        QuestionNode isAlive = new QuestionNode(IsAlive, canSee, respawning);

        rootNode = isAlive;
    }

    protected override void Update()
    {
        base.Update();
        rootNode.Execute();
        meleeEnemyFsm.Execute();

        // Debug.Log(meleeEnemyFsm.CurrentState);
    }

    public bool IdleFinished() => _idleState != null && _idleState.IdleFinished;
    public bool PatrolNeedsRest() => _patrolState != null && _patrolState.ShouldRest;


}
