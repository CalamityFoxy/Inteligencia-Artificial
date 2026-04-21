
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class EnemyController : MonoBehaviour
{
    [Header("References")]
    public Transform Target;
    public LIneOfSight los;

    [Header("Movement")]
    public float speed;
    public Vector3 currentSpeed;
    protected float speedIdle = 6f;

    [Header("Perception")]
    [SerializeField] private float perceptionInterval = 0.2f;
    [SerializeField] private float loseSightDelay = 1.5f;

    [Header("ObstacleAvoidance")]
    [SerializeField] private float obstacleAvoidanceRadius;
    [SerializeField] private float obstacleAvoidanceAngle;
    [SerializeField] private float obstacleAvoidancePersonalArea;
    [SerializeField] private LayerMask obstacleAvoidanceMask;
    [SerializeField] private Collider[]  obstacleAvoidanceColliders;
    
    [Header("References")]
    public Transform homePoint;

    public bool CanSeeTarget { get; private set; }

    private Rigidbody _rb;
    private FSM _fsm;
    private ObstacleAvoidance obstacleAvoidance;
    private float _perceptionTimer;
    private float _loseSightTimer;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {

        var idle = new EnemyIdleState(this);
        var chase = new EnemyChaseState(this);
        obstacleAvoidance = new ObstacleAvoidance(transform, obstacleAvoidanceRadius, obstacleAvoidanceAngle, obstacleAvoidancePersonalArea, obstacleAvoidanceMask);

        _fsm = new FSM();

        // acá agregamos las transiciones que vamos a tener, en nuestro diccionario va del From ( estado actual) + condición booleana si se cumple va al To(estado que queremos que vaya).
        _fsm.AddTransition(idle, () => CanSeeTarget, chase);


        _fsm.AddTransition(chase, () => ShouldLoseTarget(), idle);


        _fsm.SetInitialState(idle);
    }

    private void Update()
    {
        UpdatePerception();
        _fsm.UpdateStatesFSM();
         Debug.Log(_fsm.CurrentState);
    }


    private void UpdatePerception()
    {
        /*  _perceptionTimer += Time.deltaTime;
          if (_perceptionTimer < perceptionInterval) return;
          _perceptionTimer = 0f;*/

        CanSeeTarget =
            los.CheckRange(Target) &&
            los.CheckAngle(Target) &&
            los.CheckView(Target);
    }


    public bool ShouldLoseTarget()
    {
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
        dir = obstacleAvoidance.GetDir(dir).NoY();
        var desired_velocity = (Target.transform.position - transform.position).NoY().normalized * speed;
        var steering = desired_velocity - currentSpeed;

        currentSpeed += steering * Time.deltaTime;

        transform.position += currentSpeed;
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
