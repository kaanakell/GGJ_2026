using UnityEngine;

public abstract class BaseEnemy : MonoBehaviour
{
    protected Transform player;
    protected Rigidbody2D rb;
    protected SpriteRenderer sr;

    [Header("General")]
    public float detectionRange = 10f;
    public LayerMask groundLayer;

    [Header("Touch Damage")]
    public bool hasTouchDamage = true;
    public int touchDamageAmount = 1;
    private float touchTimer = 0f;

    [Header("Patrol")]
    public bool patrols = true;
    public float patrolSpeed = 2f;
    public float edgeCheckDistance = 0.3f;
    public float wallCheckDistance = 0.5f;

    protected int facingDir = 1;

    protected float stunDuration = 0f;
    protected float knockbackRecoveryTimer = 0f;

    public void ApplyStun(float duration)
    {
        stunDuration = Mathf.Max(stunDuration, duration);
    }

    protected virtual void HandleStun()
    {
        if (stunDuration > 0f)
        {
            stunDuration -= Time.deltaTime;
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            sr.color = new Color(0.6f, 0.6f, 1f, 1f);
        }
        else
        {
            sr.color = Color.white;
        }
    }

    public void StartKnockbackRecovery(float duration)
    {
        knockbackRecoveryTimer = Mathf.Max(knockbackRecoveryTimer, duration);
    }

    protected virtual void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (touchTimer > 0f)
            touchTimer -= Time.deltaTime;
    }

    protected void Flip()
    {
        facingDir *= -1;
        transform.localScale = new Vector3(facingDir, transform.localScale.y, transform.localScale.z);
    }

    protected void FacePlayer()
    {
        if (player == null) return;

        if ((player.position.x > transform.position.x && facingDir == -1) ||
            (player.position.x < transform.position.x && facingDir == 1))
        {
            Flip();
        }
    }

    protected void Patrol()
    {
        if (rb == null) return;

        rb.linearVelocity = new Vector2(facingDir * patrolSpeed, rb.linearVelocity.y);

        Vector3 rayOrigin = transform.position + Vector3.right * facingDir * 0.2f;
        RaycastHit2D groundAhead = Physics2D.Raycast(rayOrigin, Vector2.down, edgeCheckDistance, groundLayer);
        RaycastHit2D wallHit = Physics2D.Raycast(transform.position, Vector2.right * facingDir, wallCheckDistance, groundLayer);

        if (groundAhead.collider == null || wallHit.collider != null)
        {
            Flip();
        }
    }

    protected bool PlayerInDetectionRange()
    {
        if (player == null) return false;

        return Vector2.Distance(transform.position, player.position) <= detectionRange;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!hasTouchDamage) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                Vector2 knockDir = (playerHealth.transform.position - transform.position).normalized;
                playerHealth.TakeDamage(touchDamageAmount, knockDir);
                CameraImpulseManager.Instance.PlayImpulse(-knockDir, CameraImpulsePresets.TouchDamage);
            }
        }
    }
}