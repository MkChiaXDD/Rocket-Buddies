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

        float angle = Mathf.Atan2(this.dir.y, this.dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void Update()
    {
        transform.position += (Vector3)(dir * bulletSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Boss") || collision.gameObject.CompareTag("Bullet") || collision.gameObject.CompareTag("Pylon"))
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
