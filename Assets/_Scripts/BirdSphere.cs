using UnityEngine;

public class BirdSphere : MonoBehaviour
{
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private float damage = 100f;
    [SerializeField] private LayerMask damageLayers;
    [SerializeField] private GameObject explosionEffect;
    private void OnCollisionEnter(Collision collision)
    {
        Explode();
    }

    private void Explode()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            explosionRadius,
            damageLayers
        );

        GameObject fx = Instantiate(
               explosionEffect,
               transform.position,
               Quaternion.identity);

        Destroy(fx, .2f);

        foreach (Collider hit in hits)
        {
            if (hit.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(damage);
                if (hit.attachedRigidbody != null)
                {
                    hit.attachedRigidbody.AddExplosionForce(
                        1000f,
                        transform.position,
                        explosionRadius
                    );
                }
            }
        }
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
