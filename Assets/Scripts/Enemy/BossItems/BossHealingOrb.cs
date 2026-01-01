using UnityEngine;

public class BossHealingOrb : MonoBehaviour
{
    private Transform bossPos;
    private BossHealOrbPool pool;
    [SerializeField] private float moveSpeed = 3f;

    private void OnEnable()
    {
        pool = FindFirstObjectByType<BossHealOrbPool>();
    }

    private void Update()
    {
        if (bossPos == null)
        {
            GameObject boss = GameObject.FindGameObjectWithTag("Boss");
            if (boss == null) return;

            bossPos = boss.transform;
        }

        Debug.Log("Found boss pos");
        transform.position = Vector2.MoveTowards(transform.position, bossPos.position, moveSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Boss"))
        {
            collision.gameObject.GetComponent<BossHealthManager>()?.Heal();
            pool.ReturnObject(gameObject);
        }
    }
}
