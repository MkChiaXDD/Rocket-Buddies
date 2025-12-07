using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class RocketBullet : MonoBehaviour
{
    [Header("Basic")]
    [SerializeField] private float maxLifetime = 4f;
    [SerializeField] private LayerMask affectedLayers;

    [Header("Feel")]
    [SerializeField, Range(0f, 1f)]
    private float upwardBias = 0.15f;

    [Header("Player Colours (Hex)")]
    [SerializeField] private string player1Hex = "#00A2FF"; // blue
    [SerializeField] private string player2Hex = "#FF4747"; // red

    private Color player1Color;
    private Color player2Color;

    private float speed;
    private Vector2 dir;
    private float explosionForce;
    private float explosionRadius;
    private BulletPool bulletPool;
    private RedParticlePool redParticlePool;
    private BlueParticlePool blueParticlePool;

    private Rigidbody2D rb;
    private GameObject owner;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        // Parse hex strings into Color once
        if (!ColorUtility.TryParseHtmlString(player1Hex, out player1Color))
            player1Color = Color.blue;   // fallback

        if (!ColorUtility.TryParseHtmlString(player2Hex, out player2Color))
            player2Color = Color.red;    // fallback
    }

    // Reset everything needed when this rocket is reused from the pool
    private void ResetState()
    {
        CancelInvoke();
        StopAllCoroutines();

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        rb.simulated = true;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        // re-enable visuals & colliders
        foreach (var r in GetComponentsInChildren<SpriteRenderer>(true))
            r.enabled = true;

        foreach (var c in GetComponentsInChildren<Collider2D>(true))
            c.enabled = true;

        // lazily find particle pools
        if (redParticlePool == null)
            redParticlePool = FindFirstObjectByType<RedParticlePool>();

        if (blueParticlePool == null)
            blueParticlePool = FindFirstObjectByType<BlueParticlePool>();
    }

    // Called from PlayerController when spawning the rocket
    public void Init(
        float speed,
        Vector2 direction,
        float explosionForce,
        float explosionRadius,
        GameObject owner,
        BulletPool pool)
    {
        ResetState();

        this.speed = speed;
        this.dir = direction.normalized;
        this.explosionForce = explosionForce;
        this.explosionRadius = explosionRadius;
        this.owner = owner;
        this.bulletPool = pool;

        // ???? tint the rocket based on which player fired it
        ApplyColorForOwner();

        rb.linearVelocity = dir * speed;

        IgnoreOwnerCollisions();

        if (rb.linearVelocity.sqrMagnitude > 0.0001f)
            rb.SetRotation(Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg);

        Invoke(nameof(ExplodeSelf), maxLifetime);
    }

    private void FixedUpdate()
    {
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
        AudioManager.Instance.PlaySFX("Explode", 0.5f);
    }

    private IEnumerator ExplodeSequence(Vector2 pos)
    {
        // choose particle pool based on which player fired
        ObjectPool chosenPool = null;

        if (owner != null)
        {
            if (owner.name == "Player1")
                chosenPool = blueParticlePool;  // blue explosion
            else if (owner.name == "Player2")
                chosenPool = redParticlePool;   // red explosion
        }

        if (chosenPool != null)
        {
            GameObject particleObj = chosenPool.GetObject();
            particleObj.transform.position = pos;

            var ps = particleObj.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Clear(true);
                ps.Play(true);

                StartCoroutine(ReturnParticleAfter(ps.main.duration, particleObj, chosenPool));
            }
        }

        // Disable visuals & collisions
        foreach (var r in GetComponentsInChildren<SpriteRenderer>())
            r.enabled = false;

        foreach (var c in GetComponentsInChildren<Collider2D>())
            c.enabled = false;

        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;

        // Explosion forces
        var hits = Physics2D.OverlapCircleAll(pos, explosionRadius, affectedLayers);

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

        yield return null;

        bulletPool.ReturnObject(gameObject);
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

    private IEnumerator ReturnParticleAfter(float delay, GameObject obj, ObjectPool pool)
    {
        yield return new WaitForSeconds(delay);
        if (pool != null)
            pool.ReturnObject(obj);
    }

    private void ApplyColorForOwner()
    {
        Color col = Color.white;

        if (owner != null)
        {
            if (owner.name == "Player1")
                col = player1Color;
            else if (owner.name == "Player2")
                col = player2Color;
        }

        foreach (var sr in GetComponentsInChildren<SpriteRenderer>(true))
            sr.color = col;
    }
}
