public class RangeEnemy : EnemyController
{
    QuestionNode rootNode;
    FSM rangeEnemyFsm;
    protected override void Awake()
    {
        base.Awake();
        rangeEnemyFsm = new FSM();
        rangeEnemyFsm.RegisterState(EnemyStateType.Idle, new EnemyIdleState(this, homePoint));
        rangeEnemyFsm.RegisterState(EnemyStateType.Patroll, new EnemyPatrollState(this, Target, homePoint));
        rangeEnemyFsm.SetInitialState(EnemyStateType.Idle);

        ActionNode respawning = new ActionNode(Respawn);
        ActionNode backToBase = new ActionNode(Respawn);
        ActionNode searchFlag = new ActionNode(SearchFlag);
        ActionNode attackPlayer = new ActionNode(AttackPlayer);

        var patroll = new ActionNode(() => rangeEnemyFsm.SetState(EnemyStateType.Patroll));

        QuestionNode isFLagDropped = new QuestionNode(IsFlagDropped, searchFlag, attackPlayer);
        QuestionNode isFLagOnHome = new QuestionNode(IsFlagHome, patroll, isFLagDropped);
        QuestionNode isFLagOnMe = new QuestionNode(IsFlagOnMe, backToBase, isFLagOnHome);
        QuestionNode isAlive = new QuestionNode(IsAlive, isFLagOnMe, respawning);

        rootNode = isAlive;
    }

    protected override void Update()
    {
        base.Update();  
        rootNode.Execute();
        rangeEnemyFsm.Execute();
    }
}
