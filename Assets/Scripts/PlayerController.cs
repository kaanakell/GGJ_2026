using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 10f;
    public float jumpForce = 16f;

    [Header("Game Feel")]
    public float coyoteTime = .2f;
    public float jumpBufferTime = .2f;

    [Header("Checks")]
    public Transform groundCheck;
    public float groundCheckRadius = .2f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private float horizontalInput;

    private float coyoteCounter;
    private float jumpBufferCounter;

    public bool IsSwinging { get; set; } = false;
    public bool IsGrounded { get; set; } = true;
    public bool BlockHorizontalInput { get; set; }
    public bool FacingRight { get; private set; } = true;
    public bool HasAirJump { get; private set; }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        IsGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        if (IsGrounded)
        {
            coyoteCounter = coyoteTime;
            HasAirJump = false;
        }
        else
        {
            coyoteCounter -= Time.deltaTime;
        }

        if (jumpBufferCounter > 0f)
        {
            if ((coyoteCounter > 0f && !IsSwinging) || HasAirJump)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocityX, jumpForce);

                jumpBufferCounter = 0f;
                coyoteCounter = 0f;
                HasAirJump = false;
            }
        }

        if (horizontalInput > 0)
        {
            FacingRight = true;
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (horizontalInput < 0)
        {
            FacingRight = false;
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    public void EnableAirJump()
    {
        HasAirJump = true;
    }


    void FixedUpdate()
    {
        if (!IsSwinging && !BlockHorizontalInput)
        {
            rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocityY);
        }
        else
        {
            if (horizontalInput != 0)
            {
                rb.AddForce(new Vector2(horizontalInput * moveSpeed * .5f, 0));
            }
        }
    }
}
