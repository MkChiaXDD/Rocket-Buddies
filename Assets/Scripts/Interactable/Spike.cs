using UnityEngine;

public class Spike : MonoBehaviour
{
    private HealthManager hpMgr;
    private void Start()
    {
        hpMgr = FindFirstObjectByType<HealthManager>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            hpMgr.Damage(10);
        }
    }
}
