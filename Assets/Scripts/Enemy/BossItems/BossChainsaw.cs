using UnityEngine;

public class BossChainsaw : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 360f;
    private bool hasDamagedPlayer = false;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasDamagedPlayer) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            FindFirstObjectByType<HealthManager>()?.Damage(1);
            hasDamagedPlayer = true;
        }
    }

    public void ResetDamagePlayer()
    {
        hasDamagedPlayer = false;
    }
}
