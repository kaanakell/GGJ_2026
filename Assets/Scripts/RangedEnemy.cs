using UnityEngine;

public class RangedEnemy : BaseEnemy
{
    [Header("Ranged Strategy")]
    public GameObject projectilePrefab;
    public float shootCooldown = 2f;
    public float minShootDistance = 3f;
    public float maxShootDistance = 12f;
    public float runAwayDistance = 2.5f;
    public float moveSpeed = 3f;

    [Header("Melee Defense (Body Push)")]
    public float meleeDefenseDistance = 1.2f;
    public float pushForce = 10f;
    public float pushCooldown = 1.5f;

    private float shootTimer;
    private float pushTimer;

    private enum State { Patrol, Reposition, Shoot, MeleeDefense }
    private State currentState = State.Patrol;

    protected override void Start()
    {
        base.Start();
        shootTimer = Time.time;
    }

    void Update()
    {
        base.Update();

        HandleStun();
        if (stunDuration > 0f) return;

        if (knockbackRecoveryTimer > 0f)
        {
            knockbackRecoveryTimer -= Time.deltaTime;
            return;
        }

        if (pushTimer > 0) pushTimer -= Time.deltaTime;

        bool playerDetected = PlayerInDetectionRange();

        if (playerDetected)
        {
            FacePlayer();
            float dist = Vector2.Distance(transform.position, player.position);

            if (dist <= meleeDefenseDistance && pushTimer <= 0)
            {
                currentState = State.MeleeDefense;
            }
            else if (dist < runAwayDistance)
            {
                currentState = State.Reposition;
            }
            else if (dist <= maxShootDistance)
            {
                currentState = State.Shoot;
            }
            else
            {
                currentState = State.Patrol;
            }
        }
        else
        {
            currentState = State.Patrol;
        }

        ExecuteState();
    }

    void ExecuteState()
    {
        switch (currentState)
        {
            case State.Patrol:
                if (patrols) Patrol();
                break;

            case State.Reposition:
                float moveDir = (transform.position.x > player.position.x) ? 1 : -1;
                rb.linearVelocity = new Vector2(moveDir * moveSpeed, rb.linearVelocity.y);
                break;

            case State.Shoot:
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                if (Time.time >= shootTimer)
                {
                    Shoot();
                    shootTimer = Time.time + shootCooldown;
                }
                break;

            case State.MeleeDefense:
                rb.linearVelocity = Vector2.zero;
                PerformBodyPush();
                break;
        }
    }

    private void PerformBodyPush()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, meleeDefenseDistance, LayerMask.GetMask("Player"));

        if (hit)
        {
            PlayerHealth ph = hit.GetComponent<PlayerHealth>();
            if (ph)
            {
                Vector2 pushDir = (hit.transform.position - transform.position).normalized;
                ph.TakeDamage(1, pushDir * pushForce);
                AudioManager.Instance.PlaySFX(AudioManager.Instance.soundLibrary.rangedMeleeDefend, 0.85f);
            }
        }

        pushTimer = pushCooldown;
        currentState = State.Reposition;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, meleeDefenseDistance);
    }

    private void Shoot()
    {
        Vector2 shootDir = (player.position - transform.position).normalized;
        GameObject proj = Instantiate(projectilePrefab, transform.position + (Vector3)(shootDir * 0.5f), Quaternion.identity);
        proj.GetComponent<Projectile>().Initialize(shootDir);
        AudioManager.Instance.PlaySFX(AudioManager.Instance.soundLibrary.rangedShoot, 0.8f);
    }
}