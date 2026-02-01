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
    public ComboManager comboManager;
    #endregion

    #region Settings
    [Header("Swing")]
    public LayerMask grappleableLayer;
    public float maxGrappleDistance = 15f;
    public float swingForce = 15f;

    [Header("Enemy Pull")]
    public LayerMask enemyLayer;
    public float pullForce = 20f;
    public float releaseDistance = 1.5f;

    [Header("Whip Attack")]
    public int baseDamage = 1;
    public float whipRange = 6f;
    public float whipCooldown = 0.25f;
    public float whipHitTiming = 0.8f;

    [Header("Visuals")]
    public int whipSegments = 14;
    public float whipExtendTime = 0.12f;
    public float whipHoldTime = 0.06f;
    public float whipCurlStrength = 22f;
    public float whipTwistFrequency = 8f;
    public float grappleExtendTime = 0.06f;
    #endregion

    [Header("VFX")]
    public GameObject hitSparkPrefab;

    #region Internal State
    float whipTimer;
    float whipStartTime;
    float whipProgress;
    float impactTime;
    bool whipping;
    bool hitApplied;
    Vector2 whipEnd;
    RaycastHit2D pendingHit;

    Rigidbody2D enemyTarget;
    Vector2 grapplePoint;
    bool pullingEnemy;
    bool grappleExtending;
    float grappleExtendStartTime;
    float grappleImpactTime;
    float grappleTimer = 0f;
    public float grappleCooldown = 0.3f;
    bool swingSoundPlaying;


    #endregion

    void OnEnable()
    {
        ResetGrapple();
        lineRenderer.enabled = false;
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

    void HandleInput()
    {
        if (grappleTimer > 0f) grappleTimer -= Time.deltaTime;
        if (Input.GetMouseButtonDown(1)) StartGrapple();
        if (Input.GetMouseButtonUp(1)) ResetGrapple();

        if (whipTimer > 0f) whipTimer -= Time.deltaTime;
        if (Input.GetMouseButtonDown(0) && whipTimer <= 0f) StartWhipAttack();

        if (distanceJoint.enabled)
        {
            if (Input.GetKey(KeyCode.W)) distanceJoint.distance -= Time.deltaTime * 6f;
            if (Input.GetKey(KeyCode.S)) distanceJoint.distance += Time.deltaTime * 6f;
            distanceJoint.distance = Mathf.Clamp(distanceJoint.distance, 1f, maxGrappleDistance);
        }
    }

    void StartGrapple()
    {
        if (grappleTimer > 0f) return;
        Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = (mousePos - (Vector2)transform.position).normalized;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, maxGrappleDistance, grappleableLayer | enemyLayer);
        if (!hit) return;

        lineRenderer.enabled = true;
        lineRenderer.positionCount = whipSegments;

        if (((1 << hit.collider.gameObject.layer) & enemyLayer) != 0)
        {
            enemyTarget = hit.collider.attachedRigidbody;
            pullingEnemy = true;
            playerController.IsSwinging = false;
            grappleImpactTime = Time.time;
        }
        else
        {
            grapplePoint = hit.point;
            distanceJoint.connectedAnchor = grapplePoint;
            distanceJoint.distance = Vector2.Distance(transform.position, grapplePoint);
            distanceJoint.enabled = true;

            playerController.IsSwinging = true;
            grappleExtending = true;
            grappleExtendStartTime = Time.time;

            if (!swingSoundPlaying)
            {
                AudioManager.Instance.PlaySFX(
                    AudioManager.Instance.soundLibrary.tentacleSlide,
                    0.6f,
                    0.1f
                );
                swingSoundPlaying = true;
            }
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

        swingSoundPlaying = false;

        if (!whipping) lineRenderer.enabled = false;
        playerRb.linearDamping = 0f;
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
        float input = Input.GetAxisRaw("Horizontal");
        if (input == 0f) return;

        Vector2 toAnchor = (grapplePoint - (Vector2)transform.position).normalized;
        Vector2 tangent = new Vector2(-toAnchor.y, toAnchor.x);
        float tangentialSpeed = Vector2.Dot(playerRb.linearVelocity, tangent);

        if (Mathf.Sign(tangentialSpeed) != Mathf.Sign(input)) return;

        playerRb.linearDamping = 0.5f;
        float speedFactor = Mathf.Clamp01(Mathf.Abs(tangentialSpeed) / 10f);
        playerRb.AddForce(tangent * swingForce * (0.6f + speedFactor) * Mathf.Sign(input), ForceMode2D.Force);
    }

    void StartWhipAttack()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.soundLibrary.tentacleWhip, 0.85f);
        whipTimer = whipCooldown;
        Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = (mousePos - (Vector2)transform.position).normalized;
        pendingHit = Physics2D.Raycast(transform.position, dir, whipRange, enemyLayer);
        whipEnd = (Vector2)transform.position + dir * whipRange;
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
        SetTentaclePositions(transform.position, whipEnd, whipProgress, timeSource, whipCurlStrength, whipTwistFrequency);

        if (whipProgress >= 1f && impactTime == 0f) impactTime = Time.time;
        if (impactTime > 0f && Time.time - impactTime >= whipHoldTime)
        {
            whipping = false;
            if (!distanceJoint.enabled && !pullingEnemy) lineRenderer.enabled = false;
        }
    }

    void ApplyWhipHit()
    {
        if (!pendingHit) return;
        EnemyHealth hurt = pendingHit.collider.GetComponent<EnemyHealth>();
        BaseEnemy baseEnemy = pendingHit.collider.GetComponent<BaseEnemy>();
        if (hurt == null) return;

        if (hitSparkPrefab != null)
        {
            Vector2 impactDir = (pendingHit.point - (Vector2)transform.position).normalized;
            float angle = Mathf.Atan2(impactDir.y, impactDir.x) * Mathf.Rad2Deg;

            Instantiate(hitSparkPrefab, pendingHit.point, Quaternion.Euler(0, 0, angle));
        }

        float mult = comboManager != null ? comboManager.GetDamageMultiplier(DamageType.Tentacle) : 1f;
        int finalDamage = Mathf.CeilToInt(baseDamage * mult);

        Debug.Log($"<color=cyan>Tentacle Hit!</color> Base: {baseDamage} x Combo: {mult} = <b>Final: {finalDamage}</b>");

        Vector2 knockbackDir = (pendingHit.point - (Vector2)transform.position).normalized;
        hurt.TakeDamage(finalDamage, knockbackDir * 5f, DamageType.Tentacle);

        if (comboManager != null) comboManager.RegisterHit(DamageType.Tentacle);

        if (baseEnemy != null)
        {
            baseEnemy.ApplyStun(1.2f);
            HitStop.Freeze(0.075f);
        }

        CameraImpulseManager.Instance.PlayImpulse(knockbackDir, CameraImpulsePresets.TentacleHit);
    }

    void UpdateGrappleTentacle()
    {
        if (whipping) return;
        if (!(distanceJoint.enabled || (pullingEnemy && enemyTarget != null)) || !lineRenderer.enabled) return;

        Vector2 startPos = transform.position;
        Vector2 endPos = pullingEnemy ? enemyTarget.position : grapplePoint;
        float prog = 1f;

        if (grappleExtending)
        {
            prog = Mathf.Clamp01((Time.time - grappleExtendStartTime) / grappleExtendTime);
            if (prog >= 1f) { grappleExtending = false; grappleImpactTime = Time.time; }
        }

        SetTentaclePositions(startPos, endPos, prog, grappleExtending ? Time.time : grappleImpactTime, whipCurlStrength, whipTwistFrequency);
    }

    private void SetTentaclePositions(Vector2 start, Vector2 end, float progress, float timeSource, float curlStrength, float twistFrequency)
    {
        Vector2 dir = (end - start).normalized;
        float currentLength = Vector2.Distance(start, end) * progress;
        float segmentLength = currentLength / (whipSegments - 1);
        Vector2 prev = start;

        for (int i = 0; i < whipSegments; i++)
        {
            float t = i / (float)(whipSegments - 1);
            float phase = timeSource * twistFrequency - t * 6f;
            float rad = Mathf.Sin(phase) * curlStrength * (1f - t) * Mathf.Deg2Rad;
            Vector2 segDir = new Vector2(dir.x * Mathf.Cos(rad) - dir.y * Mathf.Sin(rad), dir.x * Mathf.Sin(rad) + dir.y * Mathf.Cos(rad));
            Vector2 pos = prev + segDir * segmentLength;
            lineRenderer.SetPosition(i, pos);
            prev = pos;
        }
    }
}