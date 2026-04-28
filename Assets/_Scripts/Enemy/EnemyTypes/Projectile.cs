using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed;
    public float damageAmount;
    public float lifeTime;

    private Vector3 direction;

    public void Init(Vector3 dir)
    {
        direction = dir.normalized;
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        var damageable = other.GetComponent<IDamageable>();
        if (damageable != null) { damageable.TakeDamage(damageAmount); }
        Destroy(gameObject);
    }
}
