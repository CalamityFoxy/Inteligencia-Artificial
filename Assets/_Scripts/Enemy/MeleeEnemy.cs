


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
        ActionNode goBase = new ActionNode(returnToBase);

        var idle = new ActionNode(()=> meleeEnemyFsm.SetState(EnemyStateType.Idle));        
        var chase = new ActionNode(()=> meleeEnemyFsm.SetState(EnemyStateType.Chase));

        //QuestionNode isFlagOnMe = new QuestionNode(IsFlagOnMe,goBase,)
        QuestionNode goToIdle = new QuestionNode(IsTargetTracked, chase, idle); //Nodo testeo para ver si funciona bien esto y funciona BARBARO GRANDE LAUTIIIIIIII
       QuestionNode isAlive = new QuestionNode(IsAlive, goToIdle , respawning);


        rootNode = isAlive;


    }

    protected override void Update()
    {
        base.Update();
        rootNode.Execute();
        meleeEnemyFsm.Execute();
    }


}
