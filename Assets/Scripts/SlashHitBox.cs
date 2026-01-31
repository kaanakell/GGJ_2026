using UnityEngine;

public class SlashHitbox : MonoBehaviour
{
    [Header("Combat Stats")]
    public int damage = 1;
    public float knockbackForce = 15f;
    public float knockbackUp = 5f;

    public DamageType damageType = DamageType.Claw;

    private void OnTriggerEnter2D(Collider2D other)
    {
        EnemyHealth enemy = other.GetComponent<EnemyHealth>();

        if (enemy != null)
        {
            Vector2 dir = (other.transform.position - transform.position).normalized;

            Vector2 knockback = (dir * knockbackForce) + (Vector2.up * knockbackUp);

            enemy.TakeDamage(damage, knockback, damageType);
        }
    }
}