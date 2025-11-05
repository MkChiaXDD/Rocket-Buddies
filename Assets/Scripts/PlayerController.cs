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
    [SerializeField] private float groundAccel = 80f;   // NEW: how fast we move toward target speed
    [SerializeField] private float groundFriction = 6f; // keep your friction value

    [Header("Aiming (Gizmo Only)")]
    [Tooltip("Ignore tiny stick drift.")]
    [Range(0f, 1f)] public float aimDeadzone = 0.2f;
    [Tooltip("How far to draw the aim ray.")]
    public float aimGizmoLength = 2f;
    public Color aimGizmoColor = Color.green;
    [Tooltip("Optional: where the shot would come from. Falls back to transform if null.")]
    public Transform firePoint;

    [Header("Aiming Visuals")]
    [SerializeField] private Transform launcherTransform; // assign your rectangle object here
    [SerializeField] private float rotationSmoothness = 10f; // optional for smooth turning

    [Header("Shooting Settings")]
    [SerializeField] private RocketBullet rocketPrefab;
    [SerializeField] private float rocketSpeed = 18f;
    [SerializeField] private float rocketExplosionForce = 12f;
    [SerializeField] private float rocketExplosionRadius = 2.5f;
    [SerializeField] private float fireCooldown = 0.15f;

    private Rigidbody2D rb;
    private Vector2 movementInput;
    private Vector2 aimInput;   // right-stick value
    private bool isGrounded;
    private float lastFireTime = -999f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (isGrounded)
        {
            Vector2 v = rb.linearVelocity;

            float target = movementInput.x * maxSpeed;

            // accelerate toward target instead of snapping (preserves blast X)
            v.x = Mathf.MoveTowards(v.x, target, groundAccel * Time.fixedDeltaTime);
            rb.linearVelocity = v;

            // friction only when truly idle (no input and tiny speed)
            if (Mathf.Approximately(movementInput.x, 0f) && Mathf.Abs(v.x) < 0.05f)
            {
                v.x = Mathf.MoveTowards(v.x, 0f, groundFriction * Time.fixedDeltaTime);
                rb.linearVelocity = v;
            }
        }
        else
        {
            rb.AddForce(Vector2.right * movementInput.x * moveForce * airControlMultiplier,
                        ForceMode2D.Force);
        }
    }

    void Update()
    {
        if (!launcherTransform) return;

        if (aimInput.magnitude < aimDeadzone) return;

        Vector2 dir = aimInput.normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        Quaternion targetRot = Quaternion.Euler(0f, 0f, angle);
        launcherTransform.rotation = targetRot;
    }


    // ---- Input System Unity Events (PlayerInput = Send Messages) ----
    public void OnMove(InputAction.CallbackContext ctx)
    {
        movementInput = ctx.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && isGrounded)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            isGrounded = false;
        }
    }

    // NEW: Aim action (Value/Vector2) bound to <Gamepad>/rightStick
    public void OnAim(InputAction.CallbackContext ctx)
    {
        // store raw stick; we'll dead-zone it when drawing/using
        if (ctx.performed) aimInput = ctx.ReadValue<Vector2>();
        else if (ctx.canceled) aimInput = Vector2.zero;
    }

    // --- NEW SHOOT METHOD ---
    public void OnShoot(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        if (Time.time < lastFireTime + fireCooldown) return;

        // determine direction (fallback right if neutral)
        Vector2 dir = aimInput.sqrMagnitude > aimDeadzone * aimDeadzone
            ? aimInput.normalized
            : Vector2.right;

        // choose spawn position
        Vector3 spawnPos = firePoint ? firePoint.position : transform.position;

        // spawn and initialize rocket
        RocketBullet rocket = Instantiate(rocketPrefab, spawnPos, Quaternion.identity);
        rocket.Init(rocketSpeed, dir, rocketExplosionForce, rocketExplosionRadius, gameObject);

        lastFireTime = Time.time;
    }

    // --- Ground check ---
    private void OnCollisionEnter2D(Collision2D c)
    {
        if (c.gameObject.CompareTag("Ground")) isGrounded = true;
    }
    private void OnCollisionExit2D(Collision2D c)
    {
        if (c.gameObject.CompareTag("Ground")) isGrounded = false;
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
}
