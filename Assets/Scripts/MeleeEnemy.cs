using UnityEngine;

public class MeleeEnemy : BaseEnemy
{
    [Header("Melee Settings")]
    public bool isStationary = false;
    public float chaseSpeed = 4f;
    public float attackRange = 1.8f;
    public float attackCooldown = 1.2f;
    public int damageToPlayer = 1;

    private float attackTimer;

    private enum State { Patrol, Chase, Attack }
    private State currentState = State.Patrol;


    protected override void Start()
    {
        base.Start();
        attackTimer = Time.time;
    }

    private void Update()
    {
        base.Update();

        HandleStun();
        if (stunDuration > 0f) return;

        if (knockbackRecoveryTimer > 0f)
        {
            knockbackRecoveryTimer -= Time.deltaTime;
            return;
        }

        bool playerDetected = PlayerInDetectionRange();

        if (playerDetected)
        {
            FacePlayer();

            float distToPlayer = Vector2.Distance(transform.position, player.position);

            if (distToPlayer <= attackRange)
            {
                currentState = State.Attack;
            }
            else
            {
                currentState = State.Chase;
            }
        }
        else
        {
            currentState = State.Patrol;
        }

        switch (currentState)
        {
            case State.Patrol:
                if (patrols && !isStationary)
                    Patrol();
                else
                    rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                break;

            case State.Chase:
                if (!isStationary)
                {
                    rb.linearVelocity = new Vector2(facingDir * chaseSpeed, rb.linearVelocity.y);
                }
                break;

            case State.Attack:
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

                if (Time.time >= attackTimer)
                {
                    PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
                    if (playerHealth != null)
                    {
                        Vector2 knockDir = (player.position - transform.position).normalized;
                        AudioManager.Instance.PlaySFX(AudioManager.Instance.soundLibrary.meleeAttack, 0.8f);
                        playerHealth.TakeDamage(damageToPlayer, knockDir);
                    }

                    attackTimer = Time.time + attackCooldown;
                }
                break;
        }
    }
}