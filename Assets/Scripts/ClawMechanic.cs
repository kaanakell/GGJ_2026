using System;
using UnityEngine;

public class ClawMechanic : MonoBehaviour
{
    [Header("References")]
    public PlayerController playerController;
    public Rigidbody2D rb;
    public ComboManager comboManager;
    public LayerMask wallLayer;
    public LayerMask enemyLayer;
    public SlashVFX slashVFX;

    [Header("Combat Stats")]
    public int baseDamage = 1;
    public float slashCooldown = 0.35f;
    private float slashTimer = 0f;

    [Header("Slash Shape")]
    public Vector2 slashBoxSize = new Vector2(1.2f, 0.6f);
    public Vector2 slashUpBoxSize = new Vector2(0.8f, 1.2f);

    [Header("Downward Slash (Pogo)")]
    public float downSlashBounceForce = 18f;
    public float downSlashHorizontalDamp = 0.3f;

    [Header("Wall Interaction")]
    public float wallSlideSpeed = 2f;
    public float wallJumpForceX = 12f;
    public float wallJumpForceY = 16f;
    public float wallCheckDistance = 0.55f;
    public float wallJumpLockTime = 0.15f;

    [Header("Wall Check")]
    public Vector2 wallCheckOffset = new Vector2(0.4f, 0f);

    [Header("VFX")]
    public Transform slashSpawnPoint;
    public GameObject hitSparkPrefab;

    private int wallDir;
    private float wallJumpLockCounter;
    private bool isWallJumping;
    private bool isTouchingWall;
    private bool isWallSticking;

    void Update()
    {
        HandleWallDetection();

        if (slashTimer > 0f)
            slashTimer -= Time.deltaTime;

        if (wallJumpLockCounter > 0f)
        {
            wallJumpLockCounter -= Time.deltaTime;
            return;
        }

        HandleWallMovement();

        if (Input.GetMouseButtonDown(0))
            Slash();
    }

    private void HandleWallDetection()
    {
        int facingSign = playerController.FacingRight ? 1 : -1;

        Vector2 origin = (Vector2)transform.position +
                         new Vector2(wallCheckOffset.x * facingSign, wallCheckOffset.y);

        Vector2 checkDir = Vector2.right * facingSign;

        RaycastHit2D hit = Physics2D.Raycast(
            origin,
            checkDir,
            wallCheckDistance,
            wallLayer
        );

        isTouchingWall = hit.collider != null;

        if (isTouchingWall)
        {
            wallDir = -facingSign;
        }
        else
        {
            wallDir = 0;
        }

        Debug.DrawRay(
            origin,
            checkDir * wallCheckDistance,
            isTouchingWall ? Color.green : Color.red
        );
    }



    private void HandleWallMovement()
    {
        if (isTouchingWall && !playerController.IsGrounded)
        {
            bool holdingStick = Input.GetMouseButton(1);

            if (holdingStick)
            {
                isWallSticking = true;
                rb.gravityScale = 0f;
                float vertical = 0f;
                if (Input.GetKey(KeyCode.W)) vertical = 1f;
                else if (Input.GetKey(KeyCode.S)) vertical = -1f;
                rb.linearVelocity = new Vector2(0f, vertical * wallSlideSpeed);
            }
            else
            {
                isWallSticking = false;
                rb.gravityScale = 1f;
                float slideY = Mathf.Max(rb.linearVelocity.y, -wallSlideSpeed);
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, slideY);
            }

            if (Input.GetButtonDown("Jump"))
                ExecuteWallJump();
        }
        else
        {
            isWallSticking = false;
            rb.gravityScale = 1f;
        }
    }

    void ExecuteWallJump()
    {
        playerController.BlockHorizontalInput = true;
        Invoke(nameof(ReleaseHorizontalControl), wallJumpLockTime);

        isWallJumping = true;
        isWallSticking = false;
        wallJumpLockCounter = wallJumpLockTime;

        rb.gravityScale = 1f;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(new Vector2(wallJumpForceX * wallDir, wallJumpForceY), ForceMode2D.Impulse);
    }

    void ReleaseHorizontalControl() => playerController.BlockHorizontalInput = false;

    private void Slash()
    {
        if (slashTimer > 0f) return;

        Vector2 dir = GetSlashDirection();

        if (slashVFX != null)
        {
            SlashVFX vfx = Instantiate(slashVFX);
            Vector2 spawnPos = slashSpawnPoint != null
                ? slashSpawnPoint.position
                : transform.position;

            vfx.Play(spawnPos, dir);
        }

        Vector2 boxSize = (dir == Vector2.up || dir == Vector2.down) ? slashUpBoxSize : slashBoxSize;
        float reach = (dir == Vector2.up || dir == Vector2.down) ? boxSize.y : boxSize.x;
        Vector2 boxCenter = (Vector2)transform.position + dir * (reach * 0.6f);

        Collider2D[] hits = Physics2D.OverlapBoxAll(boxCenter, boxSize, 0f, enemyLayer);
        bool hitSomething = false;

        foreach (Collider2D col in hits)
        {
            EnemyHealth enemy = col.GetComponent<EnemyHealth>();
            if (enemy == null) continue;

            Vector2 sparkPos = col.ClosestPoint(transform.position);

            if (hitSparkPrefab != null)
            {
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                Instantiate(hitSparkPrefab, sparkPos, Quaternion.Euler(0, 0, angle));
            }

            float multiplier = comboManager != null
                ? comboManager.GetDamageMultiplier(DamageType.Claw)
                : 1f;

            int finalDamage = Mathf.CeilToInt(baseDamage * multiplier);

            Vector2 knockback = (dir * 12f) + Vector2.up * 4f;
            enemy.TakeDamage(finalDamage, knockback, DamageType.Claw);

            if (comboManager != null)
                comboManager.RegisterHit(DamageType.Claw);

            hitSomething = true;
        }

        if (hitSomething && dir == Vector2.down && !playerController.IsGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x * downSlashHorizontalDamp, 0f);
            rb.AddForce(Vector2.up * downSlashBounceForce, ForceMode2D.Impulse);
            playerController.EnableAirJump();
        }

        AudioManager.Instance.PlayRandomSFX(
            AudioManager.Instance.soundLibrary.clawSlash,
            0.9f
        );

        if (hitSomething)
        {
            HitStop.Freeze(dir == Vector2.down ? 0.07f : 0.05f);
            CameraImpulseManager.Instance.PlayImpulse(dir, CameraImpulsePresets.ClawHit);
        }

        slashTimer = slashCooldown;
    }

    private Vector2 GetSlashDirection()
    {
        if (Input.GetKey(KeyCode.S)) return Vector2.down;
        if (Input.GetKey(KeyCode.W)) return Vector2.up;
        if (Input.GetKey(KeyCode.D)) return Vector2.right;
        if (Input.GetKey(KeyCode.A)) return Vector2.left;
        return playerController.FacingRight ? Vector2.right : Vector2.left;
    }

    public void ResetClawState()
    {
        rb.gravityScale = 1f;
        isWallSticking = false;
        playerController.BlockHorizontalInput = false;
        wallJumpLockCounter = 0f;
        CancelInvoke();
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 dir = Application.isPlaying ? GetSlashDirection() : Vector2.right;
        Vector2 size = (dir == Vector2.up || dir == Vector2.down) ? slashUpBoxSize : slashBoxSize;
        Vector2 center = (Vector2)transform.position + dir * (size.x * 0.6f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(center, size);
    }
}