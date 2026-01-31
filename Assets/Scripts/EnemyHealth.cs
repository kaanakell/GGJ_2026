using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 3;
    private int currentHealth;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount, Vector2 knockbackVector)
    {
        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
            Invoke(nameof(ResetColor), 0.1f);
        }

        if (knockbackVector.sqrMagnitude > 0.01f)
        {
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(knockbackVector, ForceMode2D.Impulse);

            BaseEnemy baseEnemy = GetComponent<BaseEnemy>();
            if (baseEnemy != null)
            {
                float recoveryDuration = 0.25f; // Base time
                recoveryDuration += knockbackVector.magnitude * 0.015f;
                baseEnemy.StartKnockbackRecovery(recoveryDuration);
            }
        }
    }

    private void ResetColor()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }
    }

    private void Die()
    {
        //death anim, particles, drops
        Destroy(gameObject);
    }
}