using UnityEngine;

public abstract class BaseEnemy : MonoBehaviour
{
    protected Transform player;
    protected Rigidbody2D rb;
    protected SpriteRenderer sr;

    [Header("General")]
    public float detectionRange = 10f;
    public LayerMask groundLayer;
    public LayerMask wallLayer;

    [Header("Touch Damage")]
    public bool hasTouchDamage = true;
    public int touchDamageAmount = 1;
    private float touchTimer = 0f;

    [Header("Patrol Settings")]
    public bool patrols = true;
    public float patrolSpeed = 2f;
    public float edgeCheckDistance = 1.5f;
    public float wallCheckDistance = 0.7f;
    public Vector2 patrolRayOffset = new Vector2(0.5f, 0f);

    protected int facingDir = 1;

    protected float flipCooldown = 0f;
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

        facingDir = transform.localScale.x > 0 ? 1 : -1;
    }

    protected virtual void Update()
    {
        if (touchTimer > 0f) touchTimer -= Time.deltaTime;
        if (flipCooldown > 0f) flipCooldown -= Time.deltaTime;
    }

    protected void Flip()
    {
        if (flipCooldown > 0f) return;

        facingDir *= -1;
        transform.localScale = new Vector3(facingDir, transform.localScale.y, transform.localScale.z);

        flipCooldown = 0.5f;
    }

    protected void FacePlayer()
    {
        if (player == null) return;

        if (Mathf.Abs(player.position.x - transform.position.x) < 0.5f) return;

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

        Vector2 sensorOrigin = (Vector2)transform.position + new Vector2(patrolRayOffset.x * facingDir, patrolRayOffset.y);

        RaycastHit2D wallHit = Physics2D.Raycast(sensorOrigin, Vector2.right * facingDir, wallCheckDistance, wallLayer);

        RaycastHit2D groundAhead = Physics2D.Raycast(sensorOrigin, Vector2.down, edgeCheckDistance, groundLayer);

        Debug.DrawRay(sensorOrigin, Vector2.right * facingDir * wallCheckDistance, Color.red);
        Debug.DrawRay(sensorOrigin, Vector2.down * edgeCheckDistance, Color.green);

        if (wallHit.collider != null || groundAhead.collider == null)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
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