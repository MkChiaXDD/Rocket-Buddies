using UnityEngine;

public class Damage : MonoBehaviour
{
    private HealthManager hpMgr;
    private void Start()
    {
        hpMgr = FindFirstObjectByType<HealthManager>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            hpMgr.Damage(10);
            FindFirstObjectByType<DeathCounter>()?.IncreaseDeath(collision.gameObject.name);
        }
    }
}
