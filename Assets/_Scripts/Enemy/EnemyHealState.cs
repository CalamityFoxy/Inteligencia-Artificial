public class EnemyHealState : State
{
    private EnemyController _enemy;

    public EnemyHealState(EnemyController enemy)
    {
        _enemy = enemy;
    }

    public override void Enter()
    {
    }

    public override void Execute()
    {
        _enemy.Stop();
        // no hace nada, el HealingZone lo cura automáticamente
    }

    public override void Exit()
    {
    }
}