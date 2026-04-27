using UnityEngine;

public class HealingZone : MonoBehaviour
{
    public float healRate = 20f;

    private void OnTriggerStay(Collider other)
    {
        IDamageable dmg = other.GetComponent<IDamageable>();

        if (dmg != null)
        {
            EnemyController enemy = other.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.Heal(healRate * Time.deltaTime);
            }
        }
    }
}
