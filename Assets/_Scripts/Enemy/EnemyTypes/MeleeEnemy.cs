using System.Collections.Generic;
using UnityEngine;

public enum CombatReaction
{
    Berserker,   // nunca se escapa
    Normal,      // al tener 25% se escapa
    Coward       // al perder 1/3 de su vida se va a curar
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

    [Header("Roulette Dinámica")]
    [SerializeField] private float allyDetectionRadius = 8f;
    [SerializeField] private float berserkerBonusPerAlly = 15f;
    [SerializeField] private LayerMask enemyLayer;

    public GameObject weaponObject;
    public Transform attackPivot;

    QuestionNode rootNode;
    FSM meleeEnemyFsm;
    private EnemyIdleState _idleState;
    private EnemyMelee_PatrolState _patrolState;
    private CombatReaction _currentReaction;
    private bool _hasRolledReaction = false;

    protected override void Awake()
    {
        base.Awake();

        meleeEnemyFsm = new FSM();

        _idleState = new EnemyIdleState(this, idleDuration);
        _patrolState = new EnemyMelee_PatrolState(this, patrolWaypoints, pathfinder, iterationsBeforeRest);

        // Estados existentes
        meleeEnemyFsm.RegisterState(EnemyStateType.Idle, _idleState);
        meleeEnemyFsm.RegisterState(EnemyStateType.Patroll, _patrolState);
        meleeEnemyFsm.RegisterState(EnemyStateType.Flee, new EnemyFleeState(this, healingPoint));
        meleeEnemyFsm.RegisterState(EnemyStateType.Heal, new EnemyHealState(this));
        meleeEnemyFsm.RegisterState(EnemyStateType.Chase, new EnemyMelee_ChaseState(this, Target, attackRange, attackCooldown, weaponObject, attackPivot));
        meleeEnemyFsm.RegisterState(EnemyStateType.Search, new EnemySearchState(this));

        // Estados de bandera: los tres son el mismo estado con distinto destino
        meleeEnemyFsm.RegisterState(EnemyStateType.GoToEnemyFlag,
            new EnemyGoToPointState(this, () => CTF_GameManager.Instance.GetEnemyFlag(Team).transform.position));

        meleeEnemyFsm.RegisterState(EnemyStateType.RecoverOwnFlag,
            new EnemyGoToPointState(this, () => CTF_GameManager.Instance.GetOwnFlag(Team).transform.position));

        meleeEnemyFsm.RegisterState(EnemyStateType.ReturnToBase,
            new EnemyGoToPointState(this, () => CTF_GameManager.Instance.GetBasePosition(Team)));

        // Hojas del árbol
        ActionNode respawning = new ActionNode(Respawn);
        var idle = new ActionNode(() => meleeEnemyFsm.SetState(EnemyStateType.Idle));
        var patrol = new ActionNode(() => meleeEnemyFsm.SetState(EnemyStateType.Patroll));
        var search = new ActionNode(() => meleeEnemyFsm.SetState(EnemyStateType.Search));
        var flee = new ActionNode(() => meleeEnemyFsm.SetState(EnemyStateType.Flee));
        var heal = new ActionNode(() => meleeEnemyFsm.SetState(EnemyStateType.Heal));
        var goToEnemyFlag = new ActionNode(() => meleeEnemyFsm.SetState(EnemyStateType.GoToEnemyFlag));
        var recoverOwnFlag = new ActionNode(() => meleeEnemyFsm.SetState(EnemyStateType.RecoverOwnFlag));
        var returnToBase = new ActionNode(() => meleeEnemyFsm.SetState(EnemyStateType.ReturnToBase));

        // Al ver al player tira la ruleta y decide qué personalidad tiene
        var chaseAfterRoll = new ActionNode(() =>
        {
            TryRollReaction();
            meleeEnemyFsm.SetState(EnemyStateType.Chase);
        });

        //  Rama de objetivo: qué hago cuando no hay player a la vista 
        QuestionNode idleOrPatrol = new QuestionNode(IdleFinished, patrol, idle);
        QuestionNode patrolOrRest = new QuestionNode(PatrolNeedsRest, idleOrPatrol, patrol);
        // Me tocó el rol de atacante? Sí si voy por la bandera enemiga.  Si no patrullo.
        QuestionNode amIAttacker = new QuestionNode(() => CTF_GameManager.Instance.IsAttacker(this), goToEnemyFlag, patrolOrRest);
        // Mi bandera está en casa? Si no, voy hacia ella esté donde esté (tirada o encima del ladrón)
        QuestionNode myFlagHome = new QuestionNode(IsFlagHome, amIAttacker, recoverOwnFlag);

        //  Rama de combate 
        QuestionNode seeOrSearch = new QuestionNode(() => CanSeeTarget, chaseAfterRoll, search);
        QuestionNode canSee = new QuestionNode(IsTargetTracked, seeOrSearch, myFlagHome);

        //  Curación según personalidad 
        QuestionNode isHealed = new QuestionNode(IsHealed, canSee, heal);
        QuestionNode reachedHealZone = new QuestionNode(() => IsAtHealingZone(healingPoint), isHealed, flee);
        QuestionNode shouldFlee = new QuestionNode(ShouldFleeForHealing, reachedHealZone, canSee);

        // ¿Llevo la bandera? Prioridad máxima la llevo a mi base
        QuestionNode hasFlag = new QuestionNode(IsFlagOnMe, returnToBase, shouldFlee);

        QuestionNode isAlive = new QuestionNode(IsAlive, hasFlag, respawning);

        rootNode = isAlive;
    }

    protected virtual void Start()
    {
        CTF_GameManager.Instance.RegisterAttackerCandidate(this);
        meleeEnemyFsm.SetInitialState(EnemyStateType.Patroll);
    }

    protected override void Update()
    {
        base.Update();
        rootNode.Execute();
        meleeEnemyFsm.Execute();

      
        // Si pierde al player, resetea la ruleta para volver a tirarla la próxima vez que lo vea
        if (!IsTargetTracked())
        {
            _hasRolledReaction = false;
        }
    }

    public bool IdleFinished() => _idleState != null && _idleState.IdleFinished;
    public bool PatrolNeedsRest() => _patrolState != null && _patrolState.ShouldRest;

    // El peso del Berserker sube según cuántos aliados tenga cerca (ruleta con peso dinámico)
    private CombatReaction RollCombatReaction()
    {
        int nearbyAllies = CountNearbyAllies();
        float berserkerWeight = 30f + (nearbyAllies * berserkerBonusPerAlly);

        var weights = new Dictionary<CombatReaction, float>
        {
            { CombatReaction.Berserker, berserkerWeight },
            { CombatReaction.Normal,    45f },
            { CombatReaction.Coward,    25f }
        };

        return Extensions.RouletteWheelSelection(weights);
    }

    private int CountNearbyAllies()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, allyDetectionRadius, enemyLayer);
        int count = 0;
        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject) continue;
            count++;
        }
        return count;
    }

    private void TryRollReaction()
    {
        if (!_hasRolledReaction)
        {
            _currentReaction = RollCombatReaction();
            _hasRolledReaction = true;
            Debug.Log($"Roulette result: {_currentReaction} | {gameObject.name}");
        }
    }

    // Cada personalidad tiene un umbral distinto de vida para salir a curarse
    public bool ShouldFleeForHealing()
    {
        if (!_hasRolledReaction) return false;

        float threshold;
        switch (_currentReaction)
        {
            case CombatReaction.Berserker:
                return false;              // pelea hasta morir
            case CombatReaction.Normal:
                threshold = 0.25f;
                break;
            case CombatReaction.Coward:
                threshold = 0.66f;
                break;
            default:
                return false;
        }

        return Health <= maxHealth * threshold;
    }
}