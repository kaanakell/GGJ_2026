using System;
using UnityEngine;

public class ClawMechanic : MonoBehaviour
{
    public PlayerController playerController;
    public Rigidbody2D rb;
    public LayerMask wallLayer;
    public float wallSlideSpeed = 2f;

    [Header("Melee")]
    public float slashRange = 1.5f;
    public LayerMask enemyLayer;
    public float slashCooldown = 0.35f;
    public float clawKnockbackForce = 15f;
    public float clawKnockbackUpward = 5f;
    private float slashTimer = 0f;

    [Header("Wall Jump")]
    public float wallJumpForceX = 12f;
    public float wallJumpForceY = 16f;
    public float wallCheckDistance = 0.55f;
    private int wallDir;

    [Header("Wall Jump Lock")]
    public float wallJumpLockTime = 0.15f;
    private float wallJumpLockCounter;
    private bool isWallJumping;

    private bool isTouchingWall;
    private bool isWallSticking;

    void Update()
    {
        RaycastHit2D leftHit = Physics2D.Raycast(transform.position, Vector2.left, wallCheckDistance, wallLayer);
        RaycastHit2D rightHit = Physics2D.Raycast(transform.position, Vector2.right, wallCheckDistance, wallLayer);

        bool touchingLeftWall = leftHit.collider != null;
        bool touchingRightWall = rightHit.collider != null;

        isTouchingWall = touchingLeftWall || touchingRightWall;

        if (slashTimer > 0f)
            slashTimer -= Time.deltaTime;

        if (wallJumpLockCounter > 0f)
        {
            wallJumpLockCounter -= Time.deltaTime;
            return;
        }

        if (isTouchingWall && !playerController.IsGrounded)
        {
            if (touchingLeftWall)
                wallDir = 1;
            else if (touchingRightWall)
                wallDir = -1;

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
            {
                ExecuteWallJump();
            }
        }
        else
        {
            isWallSticking = false;
            rb.gravityScale = 1f;
        }

        if (Input.GetMouseButtonDown(0))
            Slash();
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

        rb.AddForce(
            new Vector2(wallJumpForceX * wallDir, wallJumpForceY),
            ForceMode2D.Impulse
        );
    }

    void ReleaseHorizontalControl()
    {
        playerController.BlockHorizontalInput = false;
    }

    private void Slash()
    {
        if (slashTimer > 0f) return;

        Collider2D[] hitEnemies =
            Physics2D.OverlapCircleAll(transform.position, slashRange, enemyLayer);

        bool hitSomething = false;
        Vector2 impulseDir = Vector2.zero;

        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyHealth hurt = enemy.GetComponent<EnemyHealth>();
            if (hurt == null) continue;

            Vector2 dir = (enemy.transform.position - transform.position).normalized;

            Vector2 knockbackVector =
                (dir * clawKnockbackForce) + (Vector2.up * clawKnockbackUpward);

            hurt.TakeDamage(1, knockbackVector);
            hitSomething = true;
        }

        if (hitSomething)
        {
            CameraImpulseManager.Instance.PlayImpulse(impulseDir, CameraImpulsePresets.ClawHit);
            HitStop.Freeze(0.095f);
        }

        slashTimer = slashCooldown;
    }

    public void ResetClawState()
    {
        rb.gravityScale = 1f;

        isWallSticking = false;

        playerController.BlockHorizontalInput = false;

        wallJumpLockCounter = 0f;

        CancelInvoke();
    }
}