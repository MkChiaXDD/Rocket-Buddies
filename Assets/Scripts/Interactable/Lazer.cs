using System.Collections;
using UnityEngine;

public class Lazer : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private BoxCollider2D col;

    private HealthManager hpMgr;
    private void Start()
    {
        hpMgr = FindFirstObjectByType<HealthManager>();

        if (!sprite)
        {
            sprite = GetComponent<SpriteRenderer>();
        }

        if (!col)
        {
            col = GetComponent<BoxCollider2D>();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            hpMgr.Damage(10);
            FindFirstObjectByType<DeathCounter>()?.IncreaseDeath(collision.gameObject.name);
        }
    }

    public void DeactivateLazer(float duration)
    {
        StartCoroutine(TimedDeactivate(duration));
    }

    private IEnumerator TimedDeactivate(float duration)
    {
        sprite.enabled = false;
        col.enabled = false;

        yield return new WaitForSeconds(duration);

        sprite.enabled = true;
        col.enabled = true;
    }
}
