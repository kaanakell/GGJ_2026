using UnityEngine;

public class InstantKillTrap : MonoBehaviour
{
    [Header("Settings")]
    public int massiveDamage = 999;
    public bool destroyOnTrigger = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                Vector2 knockDir = (collision.transform.position - transform.position).normalized;
                playerHealth.TakeDamage(massiveDamage, knockDir);
                if (destroyOnTrigger)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}