


public class MeleeEnemy : EnemyController
{
    QuestionNode rootNode;
    FSM meleeEnemyFsm;

    private float meleeMaxHealth = 200f;

    protected override void Awake()
    {

        maxHealth = meleeMaxHealth;
        base.Awake();

        meleeEnemyFsm = new FSM();

        meleeEnemyFsm.RegisterState(EnemyStateType.Idle, new EnemyIdleState(this,homePoint));
        meleeEnemyFsm.RegisterState(EnemyStateType.Chase, new EnemyChaseState(this));
        meleeEnemyFsm.SetInitialState(EnemyStateType.Idle);

        ActionNode respawning = new ActionNode(Respawn);
        ActionNode searchFlag = new ActionNode(SearchFlag);
        ActionNode attackPlayer = new ActionNode(AttackPlayer);

        var idle = new ActionNode(()=> meleeEnemyFsm.SetState(EnemyStateType.Idle));        
        var chase = new ActionNode(()=> meleeEnemyFsm.SetState(EnemyStateType.Chase));

        // QuestionNode isFlagOnMe = new QuestionNode(IsFlagOnMe,)
        QuestionNode goToIdle = new QuestionNode(()=> CanSeeTarget, chase, idle);
        QuestionNode alive = new QuestionNode(IsAlive, goToIdle , respawning);


        rootNode = alive;


    }

    protected override void Update()
    {
        base.Update();
        rootNode.Execute();
        meleeEnemyFsm.Execute();
    }


}
