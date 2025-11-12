using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class RocketBullet : MonoBehaviour
{
    [Header("Basic")]
    [SerializeField] private float maxLifetime = 4f;
    [SerializeField] private LayerMask affectedLayers;   // e.g. Player, Default

    [Header("Feel")]
    [SerializeField, Range(0f, 1f)]
    private float upwardBias = 0.15f; // small extra up for nicer jumps

    // Set via Init()
    private float speed;
    private Vector2 dir;
    private float explosionForce;
    private float explosionRadius;

    private Rigidbody2D rb;
    private GameObject owner;

    // --- Called from PlayerController when spawning the rocket ---
    public void Init(float speed, Vector2 direction, float explosionForce, float explosionRadius, GameObject owner)
    {
        this.speed = speed;
        this.dir = direction.normalized;
        this.explosionForce = explosionForce;
        this.explosionRadius = explosionRadius;
        this.owner = owner;

        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = dir * speed;

        IgnoreOwnerCollisions();

        // face travel direction immediately
        if (rb.linearVelocity.sqrMagnitude > 0.0001f)
            rb.SetRotation(Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg);

        Invoke(nameof(ExplodeSelf), maxLifetime);
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    private void FixedUpdate()
    {
        // rotate rocket to face its current velocity
        Vector2 v = rb.linearVelocity;
        if (v.sqrMagnitude > 0.0001f)
            rb.SetRotation(Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg);
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (owner != null && col.collider.transform.root == owner.transform.root)
            return;

        Explode(col.GetContact(0).point);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Target"))
        {
            var script = collision.GetComponent<Target>();
            if (script && !script.GetIsHit())
            {
                Explode(collision.transform.position);
                script.OnHit();
            }
        }
    }

    private void ExplodeSelf() => Explode(transform.position);

    private void Explode(Vector2 pos)
    {
        StartCoroutine(ExplodeSequence(pos));
    }

    private IEnumerator ExplodeSequence(Vector2 pos)
    {
        // Disable visuals & collisions
        foreach (var r in GetComponentsInChildren<SpriteRenderer>())
            r.enabled = false;

        foreach (var c in GetComponentsInChildren<Collider2D>())
            c.enabled = false;

        // Stop movement
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;

        // Apply explosion forces
        var hits = Physics2D.OverlapCircleAll(pos, explosionRadius, affectedLayers);

        // 1) pick one closest collider per Rigidbody2D
        var closest = new Dictionary<Rigidbody2D, Collider2D>();
        foreach (var c in hits)
        {
            var body = c.attachedRigidbody;
            if (!body) continue;

            Vector2 p = c.ClosestPoint(pos);
            if (!closest.ContainsKey(body))
            {
                closest[body] = c;
            }
            else
            {
                Vector2 prevP = closest[body].ClosestPoint(pos);
                if ((p - pos).sqrMagnitude < (prevP - pos).sqrMagnitude)
                    closest[body] = c;
            }
        }

        // 2) apply impulse once per body
        foreach (var kv in closest)
        {
            var body = kv.Key;
            var col = kv.Value;

            Vector2 nearest = col.ClosestPoint(pos);
            float dist = Mathf.Max(0.05f, (nearest - pos).magnitude);

            float t = dist / explosionRadius;
            float falloff = Mathf.Clamp01(1f - t * t);

            Vector2 dir = ((Vector2)body.worldCenterOfMass - pos).normalized;
            Vector2 impulse = dir * (explosionForce * falloff);
            impulse += Vector2.up * (explosionForce * upwardBias * falloff);

            body.AddForce(impulse, ForceMode2D.Impulse);
        }

        yield return null; // small delay, ensures impulses applied before destruction
        Destroy(gameObject);
    }

    private void IgnoreOwnerCollisions()
    {
        if (!owner) return;

        var myCols = GetComponentsInChildren<Collider2D>(true);
        var ownerCols = owner.GetComponentsInChildren<Collider2D>(true);

        foreach (var a in myCols)
            foreach (var b in ownerCols)
                Physics2D.IgnoreCollision(a, b, true);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.25f);
        Gizmos.DrawSphere(transform.position, explosionRadius);
    }
}
