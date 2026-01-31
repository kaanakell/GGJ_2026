using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 3;
    [SerializeField]
    private int currentHealth;

    [Header("Resistances & Weaknesses")]
    public DamageType weakness = DamageType.Claw;
    public DamageType resistance = DamageType.Tentacle;

    [Header("Feedback")]
    public float flashDuration = 0.1f;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private Color originalColor;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer) originalColor = spriteRenderer.color;
        currentHealth = maxHealth;
    }

    public void TakeDamage(int baseDamage, Vector2 knockbackVector, DamageType incomingType)
    {
        float finalDamage = baseDamage;
        bool isCritical = false;
        bool isResisted = false;

        if (incomingType == weakness)
        {
            finalDamage *= 2f;
            isCritical = true;
        }
        else if (incomingType == resistance)
        {
            finalDamage *= 0.5f;
            isResisted = true;
        }

        int damageToApply = Mathf.FloorToInt(finalDamage);

        if (damageToApply < 1 && !isResisted) damageToApply = 1;

        currentHealth -= damageToApply;

        if (rb != null)
        {
            float knockbackMult = isCritical ? 1.5f : (isResisted ? 0.5f : 1f);

            rb.linearVelocity = Vector2.zero;
            rb.AddForce(knockbackVector * knockbackMult, ForceMode2D.Impulse);

            BaseEnemy baseEnemy = GetComponent<BaseEnemy>();
            if (baseEnemy != null) baseEnemy.StartKnockbackRecovery(0.2f);
        }

        StartCoroutine(FlashRoutine(isCritical, isResisted));

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator FlashRoutine(bool crit, bool resist)
    {
        if (spriteRenderer == null) yield break;

        Color flashColor = crit ? Color.red : (resist ? Color.gray : Color.white);

        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }

    private void Die()
    {
        // Add particles here later
        Destroy(gameObject);
    }
}