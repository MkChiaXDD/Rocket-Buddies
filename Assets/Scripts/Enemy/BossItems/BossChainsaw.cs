using UnityEngine;

public class BossChainsaw : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 360f;

    private bool hasDamagedPlayer1;
    private bool hasDamagedPlayer2;

    void Update()
    {
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        // Identify which player
        if (collision.gameObject.name == "Player1" && !hasDamagedPlayer1)
        {
            DamagePlayer();
            hasDamagedPlayer1 = true;
        }
        else if (collision.gameObject.name == "Player2" && !hasDamagedPlayer2)
        {
            DamagePlayer();
            hasDamagedPlayer2 = true;
        }
    }

    private void DamagePlayer()
    {
        FindFirstObjectByType<HealthManager>()?.Damage(1);
    }

    public void ResetDamagePlayer()
    {
        hasDamagedPlayer1 = false;
        hasDamagedPlayer2 = false;
    }
}
