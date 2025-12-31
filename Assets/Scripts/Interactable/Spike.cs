using UnityEngine;

public class Spike : MonoBehaviour
{
    public bool hasHit;
    private HealthManager hpMgr;
    private void Start()
    {
        hpMgr = FindFirstObjectByType<HealthManager>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHit) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            //hpMgr.Damage(1);
            hasHit = true;
        }
    }
}
