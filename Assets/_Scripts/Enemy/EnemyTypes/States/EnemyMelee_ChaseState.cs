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
       
        attackTimer = attackCooldown;

        
        if (weaponObject != null) weaponObject.SetActive(false);
        isSwinging = false;
    }

    public override void Execute()
    {
        attackTimer += Time.deltaTime;

        float distance = Vector3.Distance(target.position, _enemy.transform.position);
        Vector3 dir = _enemy.LastKnownTargetPosition - _enemy.transform.position;

       
        if (isSwinging)
        {
            UpdateSwing();
        }

        
        if (distance > attackRange && _enemy.IsTargetInLos())
        {
            _enemy.MoveWithSteering(dir);
            _enemy.Look(dir.NoY());
        }
        else
        {
            
            _enemy.Stop();
            _enemy.Look(dir.NoY());

            if (attackTimer >= attackCooldown && !isSwinging)
            {
                StartAttack();
                attackTimer = 0f;
            }
        }
    }

    private void StartAttack()
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