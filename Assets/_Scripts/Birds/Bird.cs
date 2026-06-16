using UnityEngine;

public class Bird : MonoBehaviour
{
    [HideInInspector] public BirdSpawner flock;
    [HideInInspector] public Vector3 velocity;

    [Header("Movement")]
    [SerializeField] private float speed = 8f;
    [SerializeField] private float steeringSpeed = 3f;

    [Header("Flocking")]
    [SerializeField] private float neighbourRadius = 8f;

    [SerializeField] private float separationWeight = 2f;
    [SerializeField] private float alignmentWeight = 1f;
    [SerializeField] private float cohesionWeight = 1f;
    [SerializeField] private float targetWeight = 3f;

    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform dropPoint;
    [SerializeField] float dropTime;
    private bool hasDropped;
    private float timer;

    private void Start()
    {
        velocity = transform.forward * speed;
        timer = 0f;
        hasDropped = false;
        dropTime = Random.Range(2f, 5f);
    }
    private void DropProjectile()
    {
        Instantiate(
            projectilePrefab,
            dropPoint.position,
            Quaternion.identity);
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (!hasDropped && timer >= dropTime)
        {
            DropProjectile();
            hasDropped = true;
        }

        if (flock == null)
            return;

        Vector3 separation = Vector3.zero;
        Vector3 alignment = Vector3.zero;
        Vector3 cohesion = Vector3.zero;

        int neighbours = 0;

        foreach (Bird other in flock.Birds)
        {
            if (other == this)
                continue;

            float dist = Vector3.Distance(transform.position, other.transform.position);

            if (dist > neighbourRadius)
                continue;

            neighbours++;

            separation += (transform.position - other.transform.position).normalized / Mathf.Max(dist, 0.1f);
            alignment += other.velocity.normalized;
            cohesion += other.transform.position;
        }

        if (neighbours > 0)
        {
            alignment /= neighbours;

            cohesion =
                ((cohesion / neighbours) - transform.position).normalized;
        }

        Vector3 target =
            (flock.Target.position - transform.position).normalized;

        Vector3 desired =
              separation * separationWeight
            + alignment * alignmentWeight
            + cohesion * cohesionWeight
            + target * targetWeight;

        desired.Normalize();

        velocity = Vector3.Lerp(
            velocity,
            desired * speed,
            steeringSpeed * Time.deltaTime);

        transform.position += velocity * Time.deltaTime;

        if (velocity.sqrMagnitude > 0.01f)
            transform.forward = velocity.normalized;
    }
}
