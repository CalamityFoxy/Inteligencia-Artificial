



using System.Collections.Generic;

using UnityEngine;
public enum CombatReaction
{
    Berserker,   // 30% este nunca se escapa
    Normal,      // 45% al tener 25% se escapa
    Coward       // 25% al perder 1/3 de su vida se las toma a curarse
}

public class MeleeEnemy : EnemyController
{
    [Header("Attack Settings")]
    public float attackRange = 2f;
    public float attackCooldown = 3f;

    public int iterationsBeforeRest = 4;
    public float idleDuration = 3f;

    [Header("Patrol")]
    public List<WaypointNode> patrolWaypoints;

    [Header("Roullete Dinamica")]
    [SerializeField] private float allyDetectionRadius = 8f;   
    [SerializeField] private float berserkerBonusPerAlly = 15f; 
    [SerializeField] private LayerMask enemyLayer;

    QuestionNode rootNode;
    FSM meleeEnemyFsm;
    private EnemyIdleState _idleState;
    private EnemyMelee_PatrolState _patrolState;
    private CombatReaction _currentReaction;
    private bool _hasRolledReaction = false;


    public GameObject weaponObject;
    public Transform attackPivot;
    protected override void Awake()
    {
        base.Awake();

        meleeEnemyFsm = new FSM();

        _idleState = new EnemyIdleState(this, idleDuration);// estos los creo para que el behaviour Tree los guarde de referencia y los cambie despues y asi la FSM no se entera de lo que esta pasando dentro del estado ni sus metodos(ni deberia).
        _patrolState = new EnemyMelee_PatrolState(this, patrolWaypoints, pathfinder, iterationsBeforeRest);// lo mismo acá



        //registramos los estados en la fsm
        meleeEnemyFsm.RegisterState(EnemyStateType.Idle, _idleState);
        meleeEnemyFsm.RegisterState(EnemyStateType.Patroll, _patrolState);
        meleeEnemyFsm.RegisterState(EnemyStateType.Flee, new EnemyFleeState(this, healingPoint));
        meleeEnemyFsm.RegisterState(EnemyStateType.Heal, new EnemyHealState(this));
        meleeEnemyFsm.RegisterState(EnemyStateType.Chase, new EnemyMelee_ChaseState(this, Target, attackRange, attackCooldown,weaponObject,attackPivot));
        meleeEnemyFsm.RegisterState(EnemyStateType.Search, new EnemySearchState(this));

        // creamos los nodos que vamos a utilizar en el BT
        ActionNode respawning = new ActionNode(Respawn);
        ActionNode searchFlag = new ActionNode(SearchFlag);
        ActionNode attackPlayer = new ActionNode(AttackPlayer);
        ActionNode goBase = new ActionNode(returnToBase);

        var idle = new ActionNode(() => meleeEnemyFsm.SetState(EnemyStateType.Idle));
        var patrol = new ActionNode(() => meleeEnemyFsm.SetState(EnemyStateType.Patroll));
        var chase = new ActionNode(() => meleeEnemyFsm.SetState(EnemyStateType.Chase));
        var flee = new ActionNode(() => meleeEnemyFsm.SetState(EnemyStateType.Flee));
        var heal = new ActionNode(() => meleeEnemyFsm.SetState(EnemyStateType.Heal));
        var search = new ActionNode(() => meleeEnemyFsm.SetState(EnemyStateType.Search));

        // aca decide que tipo de enemigo va ser cual lo ve al player
        var chaseAfterRoll = new ActionNode(() =>
        {
            TryRollReaction();
            meleeEnemyFsm.SetState(EnemyStateType.Chase);
        });

        // creamos el recorrido del BT, de raiz al ultimo
        QuestionNode idleOrPatrol = new QuestionNode(IdleFinished, patrol, idle);
        QuestionNode notSeeingPlayer = new QuestionNode(PatrolNeedsRest, idleOrPatrol, patrol);
        QuestionNode seeOrSearch = new QuestionNode(() => CanSeeTarget, chaseAfterRoll, search);
        QuestionNode canSee = new QuestionNode(IsTargetTracked, seeOrSearch, notSeeingPlayer);
        QuestionNode isHealed = new QuestionNode(IsHealed, canSee, heal);
        QuestionNode reachedHealZone = new QuestionNode(() => IsAtHealingZone(healingPoint), isHealed, flee);
        QuestionNode shouldFlee = new QuestionNode(ShouldFleeForHealing, reachedHealZone, canSee);


        QuestionNode isAlive = new QuestionNode(IsAlive, shouldFlee, respawning);

        rootNode = isAlive;

      //  meleeEnemyFsm.SetInitialState(EnemyStateType.Patroll); // seteamos el estado default a patrullar
    }

    protected override void Update()
    {
        base.Update(); 
        rootNode.Execute();
        meleeEnemyFsm.Execute();

        Debug.Log(meleeEnemyFsm.CurrentState);

        // hacemos esto en el update para que cuando lo pierda al player, pueda volver a dar roll selection(decida si va ser berseker,normal o coward) en la proxima vez que lo vea.
        if (!IsTargetTracked())
        { 
            _hasRolledReaction = false;   
        }
    }

    protected virtual void Start ()
    {
        meleeEnemyFsm.SetInitialState(EnemyStateType.Patroll); 
    }
    public bool IdleFinished() => _idleState != null && _idleState.IdleFinished;
    public bool PatrolNeedsRest() => _patrolState != null && _patrolState.ShouldRest;

    // función  que determina el % de que salgan cada uno en Wheel Roulette.
    private CombatReaction RollCombatReaction()
    {
        
        int nearbyAllies = CountNearbyAllies();

        // aumenta la probalidad de salir berseker con mayor cantidad de aliados con respecto a las otras specs.
        float berserkerWeight = 30f + (nearbyAllies * berserkerBonusPerAlly);

        var weights = new Dictionary<CombatReaction, float>
    {
        { CombatReaction.Berserker, berserkerWeight },  //
        { CombatReaction.Normal,    45f },
        { CombatReaction.Coward,    25f }
    };

        return Extensions.RouletteWheelSelection(weights);
    }

    //contamos aliados cercanos en un radio
    private int CountNearbyAllies()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, allyDetectionRadius, enemyLayer);

        int count = 0;
        foreach (var hit in hits)
        {
            // No me cuento a mí mismo
            if (hit.gameObject == gameObject) continue;
            count++;
        }
        return count;
    }

    private void TryRollReaction()   // La ejecutamos 
    {
        if (!_hasRolledReaction)
        {
            _currentReaction = RollCombatReaction();
            _hasRolledReaction = true;

        }
    }

    public bool ShouldFleeForHealing()
    {
        if (!_hasRolledReaction) return false;

        float threshold;

        // cantidad de vida para que vuelvan, dependiendo la cantidad de treshold entre 0 (sin vida) o 1f (full vida). Como habiamos dicho normal se vuelve al 25% a curarse, el coward en 1/3 y berseker pelea hasta la muerte
        switch (_currentReaction)
        {
            case CombatReaction.Berserker:
                return false;
            case CombatReaction.Normal:
                threshold = 0.25f; 
                break;
            case CombatReaction.Coward:
                threshold = 0.66f; // cantidad de vida para que vuelvan 1/3
                break;
            default:
                return false;
        }

        return Health <= maxHealth * threshold;
    }


}
