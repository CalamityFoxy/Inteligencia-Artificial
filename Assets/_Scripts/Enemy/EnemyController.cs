using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class EnemyController : MonoBehaviour
{
    public Transform target;
    public LIneOfSight los;
    public float speed;
    private Rigidbody _rb;
    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }
    private void Update()
    {
        if (los.CheckRange(target) && los.CheckAngle(target) && los.CheckView(target))
        {
            Vector3 dirToTarget = target.position - transform.position;
            Move(dirToTarget.normalized);
            dirToTarget.y = 0;
            Look(dirToTarget);
        }
        else
        {
            _rb.ResetInertiaTensor();
        }
        
    }

    public void Look(Vector3 dir)
    {
        transform.forward = dir;
    }

    public  void Move(Vector3 dir)
    {
        dir = dir.normalized;
        dir *= speed;
        dir.y = _rb.velocity.y;
        _rb.velocity = dir;
    }
}
