using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveForce = 20f;
    public float jumpForce = 10f;
    public float maxSpeed = 8f;
    public float airControlMultiplier = 0.8f; // reduce control mid-air if you want

    private Rigidbody2D rb;
    private Vector2 movementInput;
    private bool isGrounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        // --- Grounded movement uses direct linear velocity ---
        if (isGrounded)
        {
            Vector2 v = rb.linearVelocity;
            v.x = movementInput.x * maxSpeed; // direct snappy control
            rb.linearVelocity = v;

            // Apply friction when no movement input
            if (Mathf.Approximately(movementInput.x, 0f))
            {
                v.x = Mathf.MoveTowards(v.x, 0f, 6f * Time.fixedDeltaTime);
                rb.linearVelocity = v;
            }
        }
        else
        {
            // --- Airborne movement uses AddForce for momentum ---
            rb.AddForce(Vector2.right * movementInput.x * moveForce * airControlMultiplier, ForceMode2D.Force);
        }
    }


    // ---- Input System Unity Events ----
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

    private void OnCollisionEnter2D(Collision2D c)
    {
        if (c.gameObject.CompareTag("Ground"))
            isGrounded = true;
    }

    private void OnCollisionExit2D(Collision2D c)
    {
        if (c.gameObject.CompareTag("Ground"))
            isGrounded = false;
    }
}
