using UnityEngine;

public class RangedEnemy : BaseEnemy
{
    [Header("Ranged Settings")]
    public bool isStationary = true;
    public GameObject projectilePrefab;
    public float shootCooldown = 2f;
    public float minShootDistance = 2f;
    public float maxShootDistance = 12f;
    public float moveSpeed = 3f;
    public int projectileDamage = 1;

    private float shootTimer;

    private enum State { Patrol, MaintainDistance, Shoot }
    private State currentState = State.Patrol;

    protected override void Start()
    {
        base.Start();
        shootTimer = Time.time;
    }

    void Update()
    {
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

            if (distToPlayer >= minShootDistance && distToPlayer <= maxShootDistance)
            {
                currentState = State.Shoot;
            }
            else
            {
                currentState = State.MaintainDistance;
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

            case State.MaintainDistance:
                if (!isStationary)
                {
                    Vector2 dir;
                    if (Vector2.Distance(transform.position, player.position) < minShootDistance)
                        dir = (transform.position - player.position).normalized;
                    else
                        dir = (player.position - transform.position).normalized;

                    rb.linearVelocity = new Vector2(dir.x * moveSpeed, rb.linearVelocity.y);
                }
                break;

            case State.Shoot:
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

                if (Time.time >= shootTimer)
                {
                    Shoot();
                    shootTimer = Time.time + shootCooldown;
                }
                break;
        }
    }

    private void Shoot()
    {
        Vector2 shootDir = (player.position - transform.position).normalized;
        GameObject proj = Instantiate(projectilePrefab, transform.position + (Vector3)(shootDir * 0.5f), Quaternion.identity);
        proj.GetComponent<Projectile>().Initialize(shootDir);
    }
}
