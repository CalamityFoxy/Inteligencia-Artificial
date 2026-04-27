
using System;

using UnityEngine;


public class EnemyController : MonoBehaviour
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

    [Header("ObstacleAvoidance")]
    [SerializeField] private float obstacleAvoidanceRadius;
    [SerializeField] private float obstacleAvoidanceAngle;
    [SerializeField] private float obstacleAvoidancePersonalArea;
    [SerializeField] private LayerMask obstacleAvoidanceMask;
    [SerializeField] private Collider[] obstacleAvoidanceColliders;

    [Header("References")]
    public Transform homePoint;

    public bool CanSeeTarget { get; private set; }
    private bool _hasEverSeenTarget = false;

    public Vector3 LastKnownTargetPosition { get; private set; }

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


    public bool IsTargetInLos()
    {
        if (los.CheckView(Target) && los.CheckAngle(Target) && los.CheckRange(Target)) return true; else return false;
    }

    public void AttackPlayer()
    {
    }
    public bool IsTargetTracked() => !ShouldLoseTarget();
    public bool IsFlagHome() { return true; }
    public bool IsFlagOnMe() { return false; }
    public bool IsFlagDropped() { return false; }
    public bool IsAlive() => health > 0;
    public void Respawn() { }
    public void SearchFlag() { }

    public void returnToBase()
    {

    }
    public bool IsMelee() { return true; }


    private void UpdatePerception()
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


    public void Move(Vector3 dir)
    {
        dir = obstacleAvoidance.GetDir(dir, false);
        dir.y = 0f;

        Vector3 velocity = dir.normalized * speedIdle;
        velocity.y = _rb.velocity.y;
        _rb.velocity = velocity;

        Look(dir);

    }


    public void MoveWithSteering(Vector3 dir)
    {
        /*  dir = obstacleAvoidance.GetDir(dir).NoY();
          var desired_velocity = (Target.transform.position - transform.position).NoY().normalized * speed;
          var steering = desired_velocity - currentSpeed;

          currentSpeed += steering * Time.deltaTime;

          transform.position += currentSpeed;*/

        dir = obstacleAvoidance.GetDir(dir).NoY();


        Vector3 desired_velocity = dir.normalized * speed;
        Vector3 steering = desired_velocity - currentSpeed;

        //  Integracion (aceleracion -> velocidad)
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
        // Color base
        Gizmos.color = Color.yellow;

        // Radio de avoidance
        Gizmos.DrawWireSphere(transform.position, obstacleAvoidanceRadius);

        // Área personal (más chica)
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

        // Arco visual (opcional pero útil)
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

        // Colliders detectados (si los usás)
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
}
