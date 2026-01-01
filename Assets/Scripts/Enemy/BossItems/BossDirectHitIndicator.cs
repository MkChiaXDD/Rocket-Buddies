using UnityEngine;

public class BossDirectHitIndicator : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private Transform whiteCircle;

    [Header("Attack")]
    [SerializeField] private int damage = 1;
    [SerializeField] private LayerMask playerLayer;

    private float timer;
    private float duration;
    private float size;
    private bool active;

    private BossDirectHitIndicatorPool pool;

    // ================= INIT =================
    public void Init(
        Vector2 position,
        float timeTillImpact,
        float size,
        BossDirectHitIndicatorPool pool
    )
    {
        transform.position = position;
        this.duration = timeTillImpact;
        this.size = size;
        this.pool = pool;

        timer = 0f;
        active = true;

        whiteCircle.localScale = new Vector3(size, size, 1f);
        gameObject.SetActive(true);
    }

    // ================= UPDATE =================
    private void Update()
    {
        if (!active) return;

        timer += Time.deltaTime;
        float t = timer / duration;

        // shrink white circle
        whiteCircle.localScale = Vector3.Lerp(
            new Vector3(size, size, 1f),
            Vector3.zero,
            t
        );

        if (t >= 1f)
        {
            TriggerAttack();
        }
    }

    // ================= ATTACK =================
    private void TriggerAttack()
    {
        active = false;

        // ONE-TIME overlap check
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            size * 0.5f,
            playerLayer
        );

        foreach (var hit in hits)
        {
            hit.GetComponent<HealthManager>()?.Damage(damage);
        }

        pool.ReturnObject(gameObject);
    }

    // ================= DEBUG =================
    //private void OnDrawGizmosSelected()
    //{
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawWireSphere(transform.position, size * 0.5f);
    //}
}
