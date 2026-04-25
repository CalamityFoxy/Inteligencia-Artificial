public class RangeEnemy : EnemyController
{
    QuestionNode rootNode;
    FSM rangeEnemyFsm;
    private void Awake()
    {
        rangeEnemyFsm = new FSM();
        rangeEnemyFsm.RegisterState(EnemyStateType.Idle, new EnemyIdleState(this));
        rangeEnemyFsm.RegisterState(EnemyStateType.Patroll, new EnemyPatrollState(this, Target));
        rangeEnemyFsm.SetInitialState(EnemyStateType.Idle);

        ActionNode respawning = new ActionNode(Respawn);
        ActionNode backToBase = new ActionNode(Respawn);
        ActionNode searchFlag = new ActionNode(SearchFlag);
        ActionNode attackPlayer = new ActionNode(AttackPlayer);

        var patroll = new ActionNode(() => rangeEnemyFsm.SetState(EnemyStateType.Patroll));

        QuestionNode isMelee = new QuestionNode(IsMelee, searchFlag, patroll);

        QuestionNode isFLagDropped = new QuestionNode(IsFlagDropped, searchFlag, attackPlayer);
        QuestionNode isFLagOnHome = new QuestionNode(IsFlagHome, patroll, isFLagDropped);
        QuestionNode isFLagOnMe = new QuestionNode(IsFlagHome, backToBase, isFLagOnHome);
        QuestionNode isAlive = new QuestionNode(IsAlive, isFLagOnHome, respawning);

        rootNode = isAlive;
    }
}
