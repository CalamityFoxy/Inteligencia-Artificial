
using System;

using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public interface IDamageable
{
    void TakeDamage(float damage);
}
public class EnemyController : MonoBehaviour, IDamageable
{
    [Header("References")]
    public Transform Target;
    public LIneOfSight los;


    [Header("Movement")]
    public float speed;
    public Vector3 currentSpeed;
    protected float speedIdle = 3f;

    [Header("Perception")]
    [SerializeField] private float perceptionInterval = 0.2f;
    [SerializeField] private float loseSightDelay = 18f;

    [Header("Vida")]
    [SerializeField] private float health;
    [SerializeField] protected float maxHealth;
    [SerializeField] protected Transform healingPoint;


    [Header("ObstacleAvoidance")]
    [SerializeField] private float obstacleAvoidanceRadius;
    [SerializeField] private float obstacleAvoidanceAngle;
    [SerializeField] private float obstacleAvoidancePersonalArea;
    [SerializeField] private LayerMask obstacleAvoidanceMask;
    [SerializeField] private Collider[] obstacleAvoidanceColliders;
    public bool CanSeeTarget { get; private set; }
    public Vector3 LastKnownTargetPosition { get; private set; }
    public float Health { get => health; set => health = value; }

    private bool _hasEverSeenTarget = false;
    private ObstacleAvoidance obstacleAvoidance;
    private Rigidbody _rb;
    private float _perceptionTimer;
    private float _loseSightTimer;

    protected virtual void Awake()
    {
        health = maxHealth;

        _rb = GetComponent<Rigidbody>();

        obstacleAvoidance = new ObstacleAvoidance(
       transform,
       obstacleAvoidanceRadius,
       obstacleAvoidanceAngle,
       obstacleAvoidancePersonalArea,
       obstacleAvoidanceMask
   );
    }

    protected virtual void Update()
    {
        UpdatePerception();
    }


    public bool IsTargetInLos() // Comprueba si el target está en línea de visión, utilizando el sistema de Line of Sight
    {
        if (los.CheckView(Target) && los.CheckAngle(Target) && los.CheckRange(Target)) return true; else return false;
    }

    public void AttackPlayer() { }
    public bool IsTargetTracked() => !ShouldLoseTarget();
    public bool IsFlagHome() { return true; }
    public bool IsFlagOnMe() { return false; }
    public bool IsFlagDropped() { return false; }
    public bool IsAlive() => health > 0;
    public void Respawn() { }
    public void SearchFlag() { }
    public void returnToBase() { }

    private void UpdatePerception() // Actualiza la percepción del enemigo, comprobando si puede ver al target y actualizando la última posición conocida
    {
        CanSeeTarget =
            los.CheckRange(Target) &&
            los.CheckAngle(Target) &&
            los.CheckView(Target);

        if (CanSeeTarget)
        {
            _hasEverSeenTarget = true;
            LastKnownTargetPosition = Target.position;
        }
    }

    public bool ShouldLoseTarget()
    {

        if (!_hasEverSeenTarget) return true;

        if (CanSeeTarget)
        {
            _loseSightTimer = 0f;
            return false;
        }

        _loseSightTimer += Time.deltaTime;
        return _loseSightTimer >= loseSightDelay;
    }


    public void Move(Vector3 dir)  // Movimiento directo, sin steering, pero teniendo en cuenta el obstacle avoidance para no chocar contra paredes
    {
        dir = obstacleAvoidance.GetDir(dir, false);
        dir.y = 0f;

        Vector3 velocity = dir.normalized * speedIdle;
        velocity.y = _rb.velocity.y;
        _rb.velocity = velocity;

        Look(dir);

    }


    public void MoveWithSteering(Vector3 dir)  // Utiliza el steering para moverse, teniendo en cuenta el obstacle avoidance
    {
        dir = obstacleAvoidance.GetDir(dir).NoY();

        Vector3 desired_velocity = dir.normalized * speed;
        Vector3 steering = desired_velocity - currentSpeed;

        currentSpeed += steering * Time.deltaTime;
        currentSpeed = Vector3.ClampMagnitude(currentSpeed, speed); // que no se dispare

        Vector3 vel = currentSpeed;
        vel.y = _rb.velocity.y;
        _rb.velocity = vel;
    }

    public void Stop()
    {
        _rb.velocity = new Vector3(0, _rb.velocity.y, 0);
    }

    public void Look(Vector3 dir)
    {
        if (dir != Vector3.zero)
            transform.forward = dir;
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        // Radio de avoidance
        Gizmos.DrawWireSphere(transform.position, obstacleAvoidanceRadius);

        // Área personal 
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, obstacleAvoidancePersonalArea);

        // Ángulo de detección
        Gizmos.color = Color.cyan;

        Vector3 forward = transform.forward;
        float halfAngle = obstacleAvoidanceAngle * 0.5f;

        Vector3 leftDir = Quaternion.Euler(0, -halfAngle, 0) * forward;
        Vector3 rightDir = Quaternion.Euler(0, halfAngle, 0) * forward;

        Gizmos.DrawLine(transform.position, transform.position + leftDir * obstacleAvoidanceRadius);
        Gizmos.DrawLine(transform.position, transform.position + rightDir * obstacleAvoidanceRadius);

        // Arco visual 
        int segments = 20;
        Vector3 prevPoint = transform.position + leftDir * obstacleAvoidanceRadius;

        for (int i = 1; i <= segments; i++)
        {
            float t = (float)i / segments;
            float angle = Mathf.Lerp(-halfAngle, halfAngle, t);
            Vector3 dir = Quaternion.Euler(0, angle, 0) * forward;
            Vector3 nextPoint = transform.position + dir * obstacleAvoidanceRadius;

            Gizmos.DrawLine(prevPoint, nextPoint);
            prevPoint = nextPoint;
        }

        // Colliders detectados 
        if (obstacleAvoidanceColliders != null)
        {
            Gizmos.color = Color.magenta;

            foreach (var col in obstacleAvoidanceColliders)
            {
                if (col != null)
                    Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
            }
        }
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            gameObject.SetActive(false);
        }
    }
    public bool IsLowHP()
    {
        return health <= maxHealth * 0.35f;
    }

    public bool IsHealed()
    {
        return health >= maxHealth * 0.9f;
    }

    public void Heal(float healingRate)
    {
        health += healingRate * Time.deltaTime;
        health = Mathf.Clamp(health, 0, maxHealth);
    }
    public void FleeFromTarget()  // Flee del target, utilizando object avoidance 
    {
        Vector3 dir = (transform.position - Target.position).NoY();

        dir = obstacleAvoidance.GetDir(dir);

        Vector3 desired_velocity = dir.normalized * speed;
        Vector3 steering = desired_velocity - currentSpeed;

        currentSpeed += steering * Time.deltaTime;
        currentSpeed = Vector3.ClampMagnitude(currentSpeed, speed);

        Vector3 vel = currentSpeed;
        vel.y = _rb.velocity.y;
        _rb.velocity = vel;

        Look(dir.NoY());
    }
    public bool IsAtHealingZone(Transform healingPoint, float threshold = 1.5f)
    {
        return Vector3.Distance(transform.position, healingPoint.position) <= threshold;
    }
}
