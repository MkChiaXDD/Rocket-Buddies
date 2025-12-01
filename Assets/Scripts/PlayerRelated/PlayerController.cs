using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveForce = 20f;
    public float jumpForce = 10f;
    public float maxSpeed = 8f;
    public float airControlMultiplier = 0.8f;
    [SerializeField] private float groundAccel = 80f;
    [SerializeField] private float groundFriction = 6f;
    [SerializeField, Range(0f, 1f)] private float moveDeadzone = 0.2f;

    [Header("Air Steering")]
    [SerializeField] private float airAccelWith = 0.35f;     
    [SerializeField] private float airAccelAgainst = 1.6f;   
    [SerializeField] private float airAccelNeutral = 1.0f;   
    [SerializeField] private float neutralSpeedThreshold = 0.2f;
    private Vector2 lastAimDir = Vector2.right;

    [Header("Aiming (Gizmo Only)")]
    [Range(0f, 1f)] public float aimDeadzone = 0.2f;
    public float aimGizmoLength = 2f;
    public Color aimGizmoColor = Color.green;
    public Transform firePoint;

    [Header("Aiming Visuals")]
    [SerializeField] private Transform launcherTransform;
    [SerializeField] private float rotationSmoothness = 10f;
    [SerializeField] private GameObject longerRedLine;

    [Header("Shooting Settings")]
    [SerializeField] private RocketBullet rocketPrefab;
    [SerializeField] private float rocketSpeed = 18f;
    [SerializeField] private float rocketExplosionForce = 12f;
    [SerializeField] private float rocketExplosionRadius = 2.5f;
    [Tooltip("Time between shots for both tap and hold (seconds).")]
    [SerializeField] private float fireCooldown = 0.15f;
    private ObjectPool rocketPool;

    [Header("Ground Check")]
    [SerializeField] private float groundCheckDistance = 0.5f;
    [SerializeField] private LayerMask groundLayers;
    [SerializeField] private Transform groundCheckPoint;

    private Rigidbody2D rb;
    private Vector2 movementInput;
    private Vector2 aimInput;
    private bool isGrounded;
    private bool isAiming = false;
    private float lastMoveDir = 1f;

    // --- NEW: tap + hold state ---
    private bool isFireHeld = false;
    private float lastFireTime = -999f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rocketPool = FindFirstObjectByType<ObjectPool>();
    }

    void FixedUpdate()
    {
        isGrounded = CheckGrounded();

        if (isGrounded)
        {
            Vector2 v = rb.linearVelocity;
            float target = movementInput.x * maxSpeed;
            v.x = Mathf.MoveTowards(v.x, target, groundAccel * Time.fixedDeltaTime);
            rb.linearVelocity = v;

            if (Mathf.Approximately(movementInput.x, 0f) && Mathf.Abs(v.x) < 0.05f)
            {
                v.x = Mathf.MoveTowards(v.x, 0f, groundFriction * Time.fixedDeltaTime);
                rb.linearVelocity = v;
            }
        }
        else
        {
            float factor = GetAirControlFactor(movementInput.x);
            rb.AddForce(Vector2.right * movementInput.x * moveForce * airControlMultiplier * factor,
                        ForceMode2D.Force);
        }

        if (launcherTransform) lastAimDir = launcherTransform.right;

        if (!isAiming)
        {
            if (movementInput.x > 0.01f) lastMoveDir = 1f;
            else if (movementInput.x < -0.01f) lastMoveDir = -1f;
        }
    }

    private float GetAirControlFactor(float inputX)
    {
        if (Mathf.Approximately(inputX, 0f)) return 0f;

        float vx = rb.linearVelocity.x;
        if (Mathf.Abs(vx) < neutralSpeedThreshold) return airAccelNeutral;

        // >0 if same direction, <0 if opposite
        float sameDir = Mathf.Sign(inputX) * Mathf.Sign(vx);
        return (sameDir > 0f) ? airAccelWith : airAccelAgainst;
    }

    void Update()
    {

        if (aimInput.sqrMagnitude >= aimDeadzone * aimDeadzone)
            lastAimDir = aimInput.normalized;

        // Aim rotation (don’t early-return; we still want hold-fire below)
        if (launcherTransform && aimInput.magnitude >= aimDeadzone)
        {
            Vector2 dir = aimInput.normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            var targetRot = Quaternion.Euler(0f, 0f, angle);
            launcherTransform.rotation = targetRot;
        }

        // --- Hold-to-fire: auto-fire while held respecting cooldown ---
        if (isFireHeld && Time.time >= lastFireTime + fireCooldown)
        {
            TryFire();
        }
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        Vector2 raw = ctx.ReadValue<Vector2>();

        // Apply deadzone on X only (for horizontal movement)
        movementInput.x = Mathf.Abs(raw.x) < moveDeadzone ? 0f : raw.x;

        // Optional: if you want vertical input too (for ladders, menus, etc.)
        movementInput.y = Mathf.Abs(raw.y) < moveDeadzone ? 0f : raw.y;
    }


    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && isGrounded)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            isGrounded = false;
        }
    }

    public void OnAim(InputAction.CallbackContext ctx)
    {
        Vector2 raw = ctx.ReadValue<Vector2>();

        // Apply deadzone to both X and Y of aim input
        if (raw.magnitude < moveDeadzone)
        {
            aimInput = Vector2.zero;
            isAiming = false;
            longerRedLine.SetActive(false);
        }
        else
        {
            aimInput = raw.normalized; // normalized keeps direction consistent
            isAiming = true;
            longerRedLine.SetActive(true);
        }
    }


    // --- UPDATED: tap + hold shooting ---
    public void OnShoot(InputAction.CallbackContext ctx)
    {
        // When button starts/presses: begin holding, and also fire immediately (tap)
        if (ctx.started || ctx.performed)
        {
            isFireHeld = true;

            // Tap should feel responsive; try an immediate shot if off cooldown
            if (Time.time >= lastFireTime + fireCooldown)
                TryFire();
        }

        // When button released: stop autofire
        if (ctx.canceled)
        {
            isFireHeld = false;
        }
    }

    private bool TryFire()
    {
        if (Time.time < lastFireTime + fireCooldown) return false;

        Vector2 dir = (aimInput.sqrMagnitude >= aimDeadzone * aimDeadzone)
            ? aimInput.normalized
            : lastAimDir;   // use cached aim when idle

        Vector3 spawnPos = firePoint ? firePoint.position : transform.position;

        GameObject rocketObj = rocketPool.GetObject();
        rocketObj.transform.SetPositionAndRotation(spawnPos, Quaternion.identity);

        RocketBullet rocket = rocketObj.GetComponent<RocketBullet>();
        rocket.Init(rocketSpeed, dir, rocketExplosionForce, rocketExplosionRadius, gameObject, rocketPool);

        lastFireTime = Time.time;
        return true;
    }


    // --- Ground check ---
    private bool CheckGrounded()
    {
        Vector2 origin = groundCheckPoint ? groundCheckPoint.position : (Vector2)transform.position;
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, groundCheckDistance, groundLayers);
        return hit.collider != null;
    }

    // --- Gizmo to visualize aim ---
    void OnDrawGizmos()
    {
        Vector3 origin = firePoint ? firePoint.position : transform.position;
        Vector2 dir = aimInput;
        if (dir.magnitude < aimDeadzone) return;
        dir = dir.normalized;

        Gizmos.color = aimGizmoColor;
        Gizmos.DrawLine(origin, origin + (Vector3)(dir * aimGizmoLength));
        Gizmos.DrawSphere(origin + (Vector3)(dir * aimGizmoLength), 0.05f);
    }

    public bool GetIsMoving()
    {
        return rb.linearVelocity.sqrMagnitude > 0.01f;
    }

    public bool GetIsGrounded()
    {
        return isGrounded;
    }

    public bool GetIsAiming()
    {
        return isAiming;
    }

    public Vector2 GetAimDirection()
    {
        return lastAimDir;
    }

    public float GetLastMoveDirection()
    {
        return lastMoveDir;   // +1 right, -1 left
    }

    public bool GetIsGoingUp()
    {
        return rb.linearVelocityY > 0.1f;
    }
    
    public bool GetIsGoingDown()
    {
        return rb.linearVelocityY < -0.1f;
    }
}
