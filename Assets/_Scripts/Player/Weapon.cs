using UnityEngine;

public class Weapon : MonoBehaviour
{
    public float damageAmount;
    private void OnTriggerEnter(Collider other)
    {
        var enemy = other.GetComponent<IDamageable>();
        if (enemy != null) { enemy.TakeDamage(damageAmount); }

    }
}