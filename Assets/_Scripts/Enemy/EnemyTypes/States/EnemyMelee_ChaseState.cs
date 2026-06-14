using UnityEngine;

public class EnemyMelee_ChaseState : State
{
    private EnemyController _enemy;
    private Transform target;
    private float attackRange;
    private float attackCooldown;
    private float attackTimer;

    private GameObject weaponObject;
    private Transform attackPivot;
    private float attackSpeed = 320f;
    private float attackAngle = 180f;
    private float currentSwingAngle;
    private bool isSwinging;

    public EnemyMelee_ChaseState(EnemyController enemy, Transform target, float range, float cooldown, GameObject weapon, Transform pivot)
    {
        _enemy = enemy;
        this.target = target;
        attackRange = range;
        attackCooldown = cooldown;
        weaponObject = weapon;
        attackPivot = pivot;
    }

    public override void Enter()
    {
        attackTimer = attackCooldown; // listo para atacar al instante al entrar a rango
        if (weaponObject != null) weaponObject.SetActive(false);
        isSwinging = false;
    }

    public override void Execute()
    {
        attackTimer += Time.deltaTime;

       
        float distance = Vector3.Distance(target.position, _enemy.transform.position);
        Vector3 dir = (target.position - _enemy.transform.position).NoY();

        if (isSwinging)
        {
            UpdateSwing();
        }

        if (distance > attackRange)
        {
            // Fuera de rango: persigo con steering (Pursuit/Seek)
            _enemy.MoveWithSteering(dir);
            _enemy.Look(dir);
        }
        else
        {
            // A rango: me detengo y ataco con cooldown
            _enemy.Stop();
            _enemy.Look(dir);
            if (attackTimer >= attackCooldown && !isSwinging)
            {
                Attack();
                attackTimer = 0f;
            }
        }
    }

    private void Attack()
    {
        Debug.Log("Enemy attacks!");
        if (weaponObject != null) weaponObject.SetActive(true);
        isSwinging = true;
        currentSwingAngle = 0f;
        attackPivot.rotation = _enemy.transform.rotation;
        attackPivot.Rotate(Vector3.up, -attackAngle / 2f);
    }

    private void UpdateSwing()
    {
        float step = attackSpeed * Time.deltaTime;
        currentSwingAngle += step;
        attackPivot.Rotate(Vector3.up * step);
        if (currentSwingAngle >= attackAngle)
        {
            isSwinging = false;
            attackPivot.localRotation = Quaternion.identity;
            if (weaponObject != null) weaponObject.SetActive(false);
        }
    }

    public override void Exit()
    {
        _enemy.Stop();
        if (weaponObject != null) weaponObject.SetActive(false);
        isSwinging = false;
    }
}