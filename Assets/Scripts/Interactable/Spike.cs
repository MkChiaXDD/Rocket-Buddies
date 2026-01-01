using UnityEngine;

public class Spike : MonoBehaviour
{
    public bool hasHit;

    public void ResetHasHit()
    {
        hasHit = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHit) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            FindFirstObjectByType<HealthManager>()?.Damage(1);
            hasHit = true;
        }
    }
}
