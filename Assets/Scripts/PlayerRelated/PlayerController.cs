using System.Collections;
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
    private BulletPool rocketPool;

    [Header("Ground Check")]
    [SerializeField] private float groundCheckDistance = 0.5f;
    [SerializeField] private LayerMask groundLayers;
    [SerializeField] private Transform groundCheckPoint;
    private LandParticlePool landParticlePool;
    private bool wasGroundedLastFrame = true;

    [Header("Player Colours (Hex)")]
    [SerializeField] private string player1Hex = "#4287F5"; // Blue
    [SerializeField] private string player2Hex = "#F54269"; // Red/Pink

    private Rigidbody2D rb;
    private Vector2 movementInput;
    private Vector2 aimInput;
    private bool isGrounded;
    private bool isAiming = false;
    private float lastMoveDir = 1f;

    // --- NEW: tap + hold state ---
    private bool isFireHeld = false;
    private float lastFireTime = -999f;

    private bool movementDisabled = false;

    [SerializeField] private PlayerAnimationController playerAnim;
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rocketPool = FindFirstObjectByType<BulletPool>();
        landParticlePool = FindFirstObjectByType<LandParticlePool>();

        isAiming = false;
        longerRedLine.SetActive(false);
    }

    void FixedUpdate()
    {
        if (movementDisabled)
            return;

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

        // Landing detection: air ? ground
        if (!wasGroundedLastFrame && isGrounded)
        {
            PlayLandParticles();
        }

        wasGroundedLastFrame = isGrounded;
    }

    void Update()
    {
        if (movementDisabled)
            return;

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

    public void DisableAllMovement(bool disable)
    {
        movementDisabled = disable;

        if (disable)
        {
            // stop all motion immediately
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;

            // clear inputs
            movementInput = Vector2.zero;
            aimInput = Vector2.zero;
            isFireHeld = false;

            playerAnim.PlayDie();
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

    public void OnMove(InputAction.CallbackContext ctx)
    {
        if (movementDisabled)
            return;
        Vector2 raw = ctx.ReadValue<Vector2>();

        // Apply deadzone on X only (for horizontal movement)
        movementInput.x = Mathf.Abs(raw.x) < moveDeadzone ? 0f : raw.x;

        // Optional: if you want vertical input too (for ladders, menus, etc.)
        movementInput.y = Mathf.Abs(raw.y) < moveDeadzone ? 0f : raw.y;
    }


    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (movementDisabled)
            return;
        if (ctx.performed && isGrounded)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            isGrounded = false;
            AudioManager.Instance.PlaySFX("Jump");
        }
    }

    public void OnAim(InputAction.CallbackContext ctx)
    {
        if (movementDisabled)
            return;
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
        if (movementDisabled)
            return;
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

        AudioManager.Instance.PlaySFX("Shoot", 0.3f);

        lastFireTime = Time.time;
        return true;
    }


    // --- Ground check ---
    private bool CheckGrounded()
    {
        Vector2 origin = groundCheckPoint ? groundCheckPoint.position : (Vector2)transform.position;

        Vector2 boxSize = new Vector2(0.6f, 0.1f); // width = feet width, height = thickness
        float castDistance = groundCheckDistance;

        RaycastHit2D hit = Physics2D.BoxCast(
            origin,
            boxSize,
            0f,
            Vector2.down,
            castDistance,
            groundLayers
        );

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

    private void PlayLandParticles()
    {
        if (landParticlePool == null) return;

        GameObject particleObj = landParticlePool.GetObject();

        // place at feet
        Vector3 pos = groundCheckPoint ? groundCheckPoint.position : transform.position;
        particleObj.transform.position = pos;

        // Pick hex based on which player this is
        string hex = (gameObject.name == "Player1") ? player1Hex : player2Hex;

        // Convert hex ? Color
        Color c = Color.white;
        ColorUtility.TryParseHtmlString(hex, out c);

        // Apply color to particle system
        var ps = particleObj.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            var main = ps.main;
            main.startColor = c;

            ps.Clear(true);
            ps.Play(true);

            StartCoroutine(ReturnLandParticle(ps.main.duration + 0.7f, particleObj));
        }
    }

    private IEnumerator ReturnLandParticle(float delay, GameObject obj)
    {
        yield return new WaitForSeconds(delay);
        landParticlePool.ReturnObject(obj);
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
