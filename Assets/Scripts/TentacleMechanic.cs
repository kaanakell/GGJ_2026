using UnityEngine;

public class TentacleMechanic : MonoBehaviour
{
    #region References
    [Header("References")]
    public PlayerController playerController;
    public Rigidbody2D playerRb;
    public DistanceJoint2D distanceJoint;
    public LineRenderer lineRenderer;
    public Camera mainCamera;
    #endregion

    #region Swing
    [Header("Swing")]
    public LayerMask grappleableLayer;
    public float maxGrappleDistance = 15f;
    public float swingForce = 15f;
    #endregion

    #region Enemy Pull
    [Header("Enemy Pull")]
    public LayerMask enemyLayer;
    public float pullForce = 20f;
    public float releaseDistance = 1.5f;
    #endregion

    #region Whip Attack
    [Header("Whip Attack")]
    public float whipRange = 6f;
    public float whipCooldown = 0.25f;
    public float whipHitTiming = 0.8f;
    #endregion

    #region Whip Visual
    [Header("Whip Visual")]
    public int whipSegments = 14;
    public float whipExtendTime = 0.12f;
    public float whipHoldTime = 0.06f;
    public float whipCurlStrength = 22f;
    public float whipTwistFrequency = 8f;
    #endregion

    [Header("Grapple Limits")]
    public float grappleCooldown = 0.3f;
    private float grappleTimer = 0f;

    #region Grapple Visual - NEW
    [Header("Grapple Visual")]
    public float grappleExtendTime = 0.06f;
    #endregion

    #region Internal State
    float whipTimer;
    float whipStartTime;
    float whipProgress;
    float impactTime;

    bool whipping;
    bool hitApplied;

    Vector2 whipStart;
    Vector2 whipEnd;
    RaycastHit2D pendingHit;

    Rigidbody2D enemyTarget;
    Vector2 grapplePoint;
    bool pullingEnemy;
    bool grappleExtending;
    float grappleExtendStartTime;
    float grappleImpactTime;
    #endregion

    #region Unity
    void OnEnable()
    {
        ResetGrapple();
        lineRenderer.enabled = false;

        grappleExtending = false;
        grappleImpactTime = 0f;
    }

    void Update()
    {
        HandleInput();
        UpdateWhip();
        UpdateGrappleTentacle();
    }

    void FixedUpdate()
    {
        UpdatePulling();
        UpdateSwingPhysics();
    }
    #endregion

    #region Input
    void HandleInput()
    {
        if (grappleTimer > 0f)
            grappleTimer -= Time.deltaTime;

        if (Input.GetMouseButtonDown(1))
            StartGrapple();

        if (Input.GetMouseButtonUp(1))
            ResetGrapple();

        if (whipTimer > 0f)
            whipTimer -= Time.deltaTime;

        if (Input.GetMouseButtonDown(0) && whipTimer <= 0f)
            StartWhipAttack();

        if (distanceJoint.enabled)
        {
            if (Input.GetKey(KeyCode.W))
                distanceJoint.distance -= Time.deltaTime * 6f;

            if (Input.GetKey(KeyCode.S))
                distanceJoint.distance += Time.deltaTime * 6f;

            distanceJoint.distance = Mathf.Clamp(distanceJoint.distance, 1f, maxGrappleDistance);
        }
    }
    #endregion

    #region Grapple
    void StartGrapple()
    {
        Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = (mousePos - (Vector2)transform.position).normalized;

        if (grappleTimer > 0f) return;

        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            dir,
            maxGrappleDistance,
            grappleableLayer | enemyLayer
        );

        if (!hit) return;

        lineRenderer.enabled = true;
        lineRenderer.positionCount = whipSegments;

        grappleExtending = true;
        grappleExtendStartTime = Time.time;
        grappleImpactTime = 0f;

        if (((1 << hit.collider.gameObject.layer) & enemyLayer) != 0)
        {
            enemyTarget = hit.collider.attachedRigidbody;
            pullingEnemy = true;
            playerController.IsSwinging = false;

            grappleExtending = false;
            grappleImpactTime = Time.time;
        }
        else
        {
            grapplePoint = hit.point;
            distanceJoint.connectedAnchor = grapplePoint;
            distanceJoint.distance = Vector2.Distance(transform.position, grapplePoint);
            distanceJoint.enabled = true;
            playerController.IsSwinging = true;
        }

        grappleTimer = grappleCooldown;
    }

    public void ResetGrapple()
    {
        pullingEnemy = false;
        enemyTarget = null;

        distanceJoint.enabled = false;
        playerController.IsSwinging = false;

        grappleExtending = false;

        if (!whipping)
            lineRenderer.enabled = false;
    }

    void UpdatePulling()
    {
        if (!pullingEnemy || enemyTarget == null) return;

        Vector2 dir = ((Vector2)transform.position - enemyTarget.position).normalized;
        enemyTarget.linearVelocity = dir * pullForce;

        if (Vector2.Distance(transform.position, enemyTarget.position) < releaseDistance)
            ResetGrapple();
    }

    void UpdateSwingPhysics()
    {
        if (!distanceJoint.enabled) return;

        float h = Input.GetAxisRaw("Horizontal");
        if (h == 0f) return;

        Vector2 dir = (grapplePoint - (Vector2)transform.position).normalized;
        Vector2 perp = new Vector2(-dir.y, dir.x);
        playerRb.AddForce(perp * h * swingForce);
    }
    #endregion

    #region Whip
    void StartWhipAttack()
    {
        whipTimer = whipCooldown;

        Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = (mousePos - (Vector2)transform.position).normalized;

        pendingHit = Physics2D.Raycast(transform.position, dir, whipRange, enemyLayer);

        whipStart = transform.position;
        whipEnd = whipStart + dir * whipRange;

        whipStartTime = Time.time;
        impactTime = 0f;
        whipProgress = 0f;
        hitApplied = false;
        whipping = true;

        lineRenderer.enabled = true;
        lineRenderer.positionCount = whipSegments;
    }

    void UpdateWhip()
    {
        if (!whipping) return;

        float elapsed = Time.time - whipStartTime;
        whipProgress = Mathf.Clamp01(elapsed / whipExtendTime);

        if (!hitApplied && whipProgress >= whipHitTiming)
        {
            ApplyWhipHit();
            hitApplied = true;
        }

        float timeSource = whipProgress < 1f ? Time.time : impactTime;

        SetTentaclePositions(whipStart, whipEnd, whipProgress, timeSource, whipCurlStrength, whipTwistFrequency);

        if (whipProgress >= 1f && impactTime == 0f)
            impactTime = Time.time;

        if (impactTime > 0f && Time.time - impactTime >= whipHoldTime)
        {
            whipping = false;
            if (!distanceJoint.enabled && !pullingEnemy)
                lineRenderer.enabled = false;
        }
    }

    void ApplyWhipHit()
    {
        if (!pendingHit) return;

        EnemyHealth hurt = pendingHit.collider.GetComponent<EnemyHealth>();
        BaseEnemy baseEnemy = pendingHit.collider.GetComponent<BaseEnemy>();

        if (hurt == null) return;

        Vector2 dir = (pendingHit.collider.transform.position - transform.position).normalized;

        hurt.TakeDamage(1, Vector2.zero);

        if (baseEnemy != null)
        {
            baseEnemy.ApplyStun(1.2f);
            HitStop.Freeze(0.075f);
        }

        CameraImpulseManager.Instance.PlayImpulse(dir, CameraImpulsePresets.TentacleHit);
    }

    #endregion

    #region Shared Tentacle Visual - NEW
    void UpdateGrappleTentacle()
    {
        if (whipping) return;

        bool isActive = distanceJoint.enabled || (pullingEnemy && enemyTarget != null);
        if (!isActive || !lineRenderer.enabled) return;

        Vector2 startPos = transform.position;
        Vector2 endPos = pullingEnemy && enemyTarget != null ? enemyTarget.position : grapplePoint;

        float prog = 1f;
        float timeSource;

        if (grappleExtending)
        {
            float elapsed = Time.time - grappleExtendStartTime;
            prog = Mathf.Clamp01(elapsed / grappleExtendTime);

            timeSource = Time.time;

            if (prog >= 1f)
            {
                grappleExtending = false;
                grappleImpactTime = Time.time;
            }
        }
        else
        {
            timeSource = grappleImpactTime;
        }

        SetTentaclePositions(startPos, endPos, prog, timeSource, whipCurlStrength, whipTwistFrequency);
    }

    private void SetTentaclePositions(Vector2 start, Vector2 end, float progress, float timeSource, float curlStrength, float twistFrequency)
    {
        if (progress <= 0f)
        {
            for (int i = 0; i < whipSegments; i++)
                lineRenderer.SetPosition(i, start);
            return;
        }

        Vector2 dir = (end - start).normalized;
        float fullLength = Vector2.Distance(start, end);
        float currentLength = fullLength * progress;

        float segmentLength = currentLength / (whipSegments - 1);

        Vector2 prev = start;
        for (int i = 0; i < whipSegments; i++)
        {
            float t = i / (float)(whipSegments - 1);

            float phase = timeSource * twistFrequency - t * 6f;
            float curl = Mathf.Sin(phase) * curlStrength * (1f - t);
            float rad = curl * Mathf.Deg2Rad;

            Vector2 segDir = new Vector2(
                dir.x * Mathf.Cos(rad) - dir.y * Mathf.Sin(rad),
                dir.x * Mathf.Sin(rad) + dir.y * Mathf.Cos(rad)
            );

            Vector2 pos = prev + segDir * segmentLength;
            lineRenderer.SetPosition(i, pos);
            prev = pos;
        }
    }
    #endregion
}