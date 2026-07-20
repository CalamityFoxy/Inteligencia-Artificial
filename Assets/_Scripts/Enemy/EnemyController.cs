using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public interface IDamageable
{
    void TakeDamage(float damage);
    void Dead();
}

public class EnemyController : MonoBehaviour, IDamageable, IFlagCarrier
{
    [Header("References")]
    public Transform Target;
    public LIneOfSight los;

    [Header("Movement")]
    public float speed;
    public Vector3 currentSpeed;
    protected float speedIdle = 3f;

    [Header("PathFinding")]
    [SerializeField] protected List<WaypointNode> currentPath;
    [SerializeField] protected int currentPathIndex;
    [SerializeField] protected AStarPathfinder pathfinder;

    [Header("Bandera")]
    [SerializeField] private Team team = Team.AI;
    [SerializeField] private Transform flagHolder;   // empty hijo donde se engancha la bandera
    private Flag currentFlag;

    [Header("Perception")]
    [SerializeField] private float loseSightDelay = 3f;

    [Header("Vida")]
    [SerializeField] private float health;
    [SerializeField] private Slider healthSlider;
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

    // ----- IFlagCarrier -----
    public Transform Transform => transform;
    public Transform FlagHolder => flagHolder;
    public Team Team => team;
    public bool HasFlag => currentFlag != null;
    public Flag CurrentFlag => currentFlag;

    public void SetFlag(Flag flag) => currentFlag = flag;
    public void ClearFlag() => currentFlag = null;

    private bool _isDead = false;
    private bool _hasEverSeenTarget = false;
    private ObstacleAvoidance obstacleAvoidance;
    private Rigidbody _rb;
    private float _loseSightTimer;

    protected virtual void Awake()
    {
        health = maxHealth;

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = health;
        }

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

    public bool IsTargetInLos() // Comprueba si el target está en línea de visión, usando el sistema de Line of Sight
    {
        if (los.CheckView(Target) && los.CheckAngle(Target) && los.CheckRange(Target)) return true; else return false;
    }

    public void AttackPlayer() { }
    public bool IsTargetTracked() => !ShouldLoseTarget();
    public bool IsAlive() => health > 0;
    public void Respawn() {; }

    // ----- Preguntas de bandera para el Behaviour Tree -----

    // ¿Estoy llevando yo la bandera enemiga?
    public bool IsFlagOnMe() => currentFlag != null;

    // ¿Mi propia bandera está en su base?
    public bool IsFlagHome()
    {
        var myFlag = CTF_GameManager.Instance.GetOwnFlag(team);
        return myFlag != null && myFlag.State == FlagState.Home;
    }

    // ¿Mi propia bandera está tirada en el piso (alguien la robó y la soltó)?
    public bool IsFlagDropped()
    {
        var myFlag = CTF_GameManager.Instance.GetOwnFlag(team);
        return myFlag != null && myFlag.State == FlagState.Dropped;
    }

    public void SearchFlag() { }
    public void returnToBase() { }

    public void Dead()
    {
        if (_isDead) return;   // ya estoy muerto, no apilo respawns
        _isDead = true;

        health = 0;
        if (healthSlider != null) healthSlider.value = health;

        // Si llevaba la bandera, la suelto donde morí (antes de teleportarme)
        if (currentFlag != null)
            currentFlag.Drop(transform.position);

        transform.position = new Vector3(0, -100, 0);
        StartCoroutine(waitAndRespawn(3.5f));
    }

    private IEnumerator waitAndRespawn(float delay)
    {
        yield return new WaitForSeconds(delay);

        health = maxHealth;
        if (healthSlider != null) healthSlider.value = health;

        transform.position = healingPoint.position;
        _isDead = false;   // vuelvo a estar vivo
    }

    private void UpdatePerception() // Actualiza la percepción: si ve al target y cuál fue su última posición conocida
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

        if (_loseSightTimer < loseSightDelay)
            _loseSightTimer += Time.deltaTime;

        return _loseSightTimer >= loseSightDelay;
    }

    public void Move(Vector3 dir)  // Movimiento directo, sin steering, pero con obstacle avoidance para no chocar contra paredes
    {
        dir = obstacleAvoidance.GetDir(dir, false);
        dir.y = 0f;

        Vector3 velocity = dir.normalized * speedIdle;
        velocity.y = _rb.velocity.y;
        _rb.velocity = velocity;

        Look(dir);
    }

    public void MoveWithSteering(Vector3 dir)  // Movimiento con steering, teniendo en cuenta el obstacle avoidance
    {
        dir = obstacleAvoidance.GetDir(dir).NoY();
        Look(dir);
        Vector3 desired_velocity = dir.normalized * speed;
        Vector3 steering = desired_velocity - currentSpeed;

        currentSpeed += steering * Time.deltaTime;
        currentSpeed = Vector3.ClampMagnitude(currentSpeed, speed); // que no se dispare

        Vector3 vel = currentSpeed;
        vel.y = 0;
        _rb.velocity = vel;
    }

    public void CalculatePathTo(Vector3 destination)
    {
        WaypointNode start = pathfinder.GetClosestNode(transform.position);
        WaypointNode end = pathfinder.GetClosestNode(destination);

        currentPath = pathfinder.FindPath(start, end);
        currentPathIndex = 0;

        // Ésto sirve para que si tiene un nodo atras mas cerca que lo ignore.
        if (currentPath != null && currentPath.Count > 1)
        {
            Vector3 toFirst = (currentPath[0].transform.position - transform.position).NoY();
            Vector3 toSecond = (currentPath[1].transform.position - transform.position).NoY();

            // y tomamos este para que arranque como primer nodo
            if (toFirst.magnitude < 1.5f || Vector3.Dot(toFirst.normalized, toSecond.normalized) < 0)
            {
                currentPathIndex = 1;
            }
        }
    }

    public bool FollowCurrentPath()
    {
        if (currentPath == null || currentPathIndex >= currentPath.Count)
        {
            Stop();
            return true;
        }

        WaypointNode node = currentPath[currentPathIndex];
        Vector3 dir = node.transform.position - transform.position;

        if (dir.NoY().magnitude < 1f)
        {
            currentPathIndex++;
            return currentPathIndex >= currentPath.Count;
        }

        Move(dir.NoY());
        return false;
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

    public void TakeDamage(float damage)
    {
        if (_isDead) return;   // muerto: ignoro daño hasta respawnear

        health -= damage;
        if (healthSlider != null) healthSlider.value = health;

        if (health <= 0)
        {
            Dead();
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
        if (healthSlider != null) healthSlider.value = health;
    }

    public void FleeFromTarget()  // Flee del target, con obstacle avoidance
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, obstacleAvoidanceRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, obstacleAvoidancePersonalArea);

        Gizmos.color = Color.cyan;

        Vector3 forward = transform.forward;
        float halfAngle = obstacleAvoidanceAngle * 0.5f;

        Vector3 leftDir = Quaternion.Euler(0, -halfAngle, 0) * forward;
        Vector3 rightDir = Quaternion.Euler(0, halfAngle, 0) * forward;

        Gizmos.DrawLine(transform.position, transform.position + leftDir * obstacleAvoidanceRadius);
        Gizmos.DrawLine(transform.position, transform.position + rightDir * obstacleAvoidanceRadius);

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

        if (obstacleAvoidanceColliders != null)
        {
            Gizmos.color = Color.magenta;

            foreach (var col in obstacleAvoidanceColliders)
            {
                if (col != null)
                    Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
            }
        }

        // Path actual de A*
        if (currentPath != null && currentPath.Count > 0)
        {
            for (int i = 0; i < currentPath.Count; i++)
            {
                if (currentPath[i] == null) continue;

                Gizmos.color = Color.green;
                Gizmos.DrawSphere(currentPath[i].transform.position, 0.4f);

                if (i < currentPath.Count - 1 && currentPath[i + 1] != null)
                    Gizmos.DrawLine(currentPath[i].transform.position, currentPath[i + 1].transform.position);
            }

            if (currentPathIndex < currentPath.Count && currentPath[currentPathIndex] != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(currentPath[currentPathIndex].transform.position, 0.6f);
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, currentPath[currentPathIndex].transform.position);
            }
        }
    }
}