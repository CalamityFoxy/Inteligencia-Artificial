
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("References")]
    public Transform Target;
    public LIneOfSight los;

    [Header("Movement")]
    public float speed = 5f;

    [Header("Perception")]
    [SerializeField] private float perceptionInterval = 0.2f; 
    [SerializeField] private float loseSightDelay = 1.5f;

    [Header("References")]
    public Transform homePoint;

    public bool CanSeeTarget { get; private set; }

    private Rigidbody _rb;
    private FSM _fsm;
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
       // Debug.Log(_fsm.CurrentState);
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
        Vector3 velocity = dir.normalized * speed;
        velocity.y = _rb.velocity.y;
        _rb.velocity = velocity;
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
}