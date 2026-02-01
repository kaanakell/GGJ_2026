using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 5;
    private int currentHealth;

    [Header("Damage Feedback")]
    public float knockbackForce = 12f;
    public float invincibilityDuration = 1.0f;
    public float flashInterval = 0.1f;

    [Header("Layers (Assign in Inspector)")]
    public LayerMask enemyLayer;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private float invincibilityTimer = 0f;
    private float flashTimer = 0f;
    private int originalLayer;
    private int enemyLayerIndex;

    [Header("VFX")]
    public GameObject deathBurstPrefab;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;

        originalLayer = gameObject.layer;
        enemyLayerIndex = Mathf.RoundToInt(Mathf.Log(enemyLayer.value, 2));
    }

    private void Update()
    {
        if (invincibilityTimer > 0f)
        {
            invincibilityTimer -= Time.deltaTime;
            flashTimer -= Time.deltaTime;

            if (flashTimer <= 0f)
            {
                sr.enabled = !sr.enabled;
                flashTimer = flashInterval;
            }

            if (invincibilityTimer <= 0f)
            {
                sr.enabled = true;
                RestoreCollisions();
            }
        }
    }

    public void TakeDamage(int amount, Vector2 knockbackDir)
    {
        if (invincibilityTimer > 0f) return;

        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        HitStop.Freeze(0.06f);

        AudioManager.Instance.PlaySFX(
            AudioManager.Instance.soundLibrary.hurt,
            1f
        );

        if (knockbackDir.sqrMagnitude > 0.01f)
        {
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(knockbackDir.normalized * knockbackForce, ForceMode2D.Impulse);

            CameraImpulseManager.Instance.PlayImpulse(-knockbackDir.normalized, CameraImpulsePresets.PlayerHurt);
        }

        StartInvincibility();
    }


    private void StartInvincibility()
    {
        invincibilityTimer = invincibilityDuration;
        flashTimer = 0f;

        Physics2D.IgnoreLayerCollision(originalLayer, enemyLayerIndex, true);
    }

    private void RestoreCollisions()
    {
        Physics2D.IgnoreLayerCollision(originalLayer, enemyLayerIndex, false);
    }

    private void Die()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.soundLibrary.gameOver, 1f);
        AudioManager.Instance.StopMusic();

        if (deathBurstPrefab != null)
        {
            Instantiate(deathBurstPrefab, transform.position, Quaternion.identity);
        }

        Debug.Log("Player Died");
        GameStateManager.Instance.SetState(GameState.PlayerDead);
        UIManager.Instance.ShowGameOver();
        gameObject.SetActive(false);
    }

    public int CurrentHealth => currentHealth;
}