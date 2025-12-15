using UnityEngine;
using UnityEngine.UI;

public abstract class EnemyBase : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] protected EnemyData data;

    protected int currHp;
    protected Transform target;

    [Header("Patrol")]
    [SerializeField] protected Transform[] waypoints;
    [SerializeField] protected float waypointReachDistance = 0.1f;

    [Header("UI")]
    [SerializeField] protected Image healthFill;
    [SerializeField] protected bool hideHealthBarWhenFull = true;

    [Header("Facing")]
    [SerializeField] protected bool defaultFacingRight = true;


    protected float attackCooldown;
    protected int currentWaypointIndex;

    // ---------------- UNITY ----------------

    protected virtual void Start()
    {
        gameObject.name = data.enemyName;
        currHp = data.maxHealth;
        attackCooldown = 0f;
        UpdateHealthBar(true);
        OnInit();
    }

    protected virtual void Update()
    {
        UpdateTarget();

        if (target != null)
        {
            if (IsInAttackRange())
                TryAttack();
            else
                ChaseTarget();
        }
        else
        {
            Patrol();
        }
    }

    protected virtual void LateUpdate()
    {
        if (healthFill != null)
        {
            Transform barRoot = healthFill.transform.parent;
            barRoot.localScale = new Vector3(
                Mathf.Abs(barRoot.localScale.x),
                barRoot.localScale.y,
                barRoot.localScale.z
            );
        }
    }


    protected virtual void UpdateHealthBar(bool forceShow = false)
    {
        if (healthFill == null) return;

        float normalizedHp = (float)currHp / data.maxHealth;
        healthFill.fillAmount = normalizedHp;

        if (hideHealthBarWhenFull && !forceShow)
        {
            healthFill.transform.parent.gameObject.SetActive(normalizedHp < 1f);
        }
    }

    protected void UpdateFacing(Vector3 destination)
    {
        float dirX = destination.x - transform.position.x;

        if (Mathf.Abs(dirX) < 0.01f) return;

        Vector3 scale = transform.localScale;

        if (dirX < 0 && scale.x > 0) // moving left
            scale.x *= -1;
        else if (dirX > 0 && scale.x < 0) // moving right
            scale.x *= -1;

        transform.localScale = scale;
    }


    // ---------------- DETECTION ----------------

    protected virtual void UpdateTarget()
    {
        float closestDist = Mathf.Infinity;
        Transform closestPlayer = null;

        PlayerController[] players = FindObjectsByType<PlayerController>(
            FindObjectsSortMode.None
        );

        foreach (var player in players)
        {
            float dist = Vector2.Distance(transform.position, player.transform.position);

            if (dist <= data.detectionRange && dist < closestDist)
            {
                closestDist = dist;
                closestPlayer = player.transform;
            }
        }

        target = closestPlayer;

        if (closestPlayer == null)
            attackCooldown = 0f;
    }

    protected bool IsInAttackRange()
    {
        return target != null &&
               Vector2.Distance(transform.position, target.position) <= data.attackRange;
    }

    // ---------------- MOVEMENT ----------------

    protected virtual void Patrol()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Transform wp = waypoints[currentWaypointIndex];
        UpdateFacing(wp.position);
        MoveTo(wp.position);

        if (Vector2.Distance(transform.position, wp.position) <= waypointReachDistance)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
    }

    protected virtual void ChaseTarget()
    {
        if (target == null) return;
        UpdateFacing(target.position);
        MoveTo(target.position);
    }

    protected abstract void MoveTo(Vector3 destination);

    // ---------------- COMBAT ----------------

    protected virtual void TryAttack()
    {
        if (attackCooldown > 0f)
        {
            attackCooldown -= Time.deltaTime;
            return;
        }

        Attack();

        // attackSpeed = seconds per attack
        attackCooldown = Mathf.Max(data.attackSpeed, 0.01f);
    }


    protected abstract void Attack();

    public virtual void OnHit(int damage)
    {
        currHp -= damage;
        currHp = Mathf.Max(currHp, 0);

        UpdateHealthBar();

        if (currHp <= 0)
            Die();
    }

    protected virtual void Die()
    {
        if (healthFill != null)
            healthFill.transform.parent.gameObject.SetActive(false);

        Destroy(gameObject);
    }


    // ---------------- INIT HOOK ----------------

    protected virtual void OnInit() { }

    // ---------------- GIZMOS ----------------

    protected virtual void OnDrawGizmosSelected()
    {
        if (data == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, data.detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, data.attackRange);

        if (waypoints != null)
        {
            Gizmos.color = Color.cyan;
            foreach (var wp in waypoints)
            {
                if (wp != null)
                    Gizmos.DrawSphere(wp.position, 0.05f);
            }
        }
    }
}
