using UnityEngine;

public class BossBullet : MonoBehaviour
{
    private Vector2 dir;
    private float bulletSpeed;
    private BossBulletPool pool;
    private void Start()
    {
        pool = FindFirstObjectByType<BossBulletPool>();
    }
    public void Init(Vector2 dir, float bulletSpeed)
    {
        this.dir = dir;
        this.bulletSpeed = bulletSpeed;
    }

    private void Update()
    {
        transform.position += (Vector3)(dir * bulletSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Boss"))
        {
            return;
        }
        if (collision.gameObject.CompareTag("Player"))
        {
            FindFirstObjectByType<HealthManager>()?.Damage(1);
            pool.ReturnObject(gameObject);
        }
        else
        {
            pool.ReturnObject(gameObject);
        }
    }
}
