using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHp = 3;
    private int currHp;

    [Header("UI")]
    [SerializeField] private Image[] hearts;

    private void Start()
    {
        currHp = maxHp;
        UpdateHearts();
    }

    // ---------------- HEALTH ----------------

    public void Heal(int amount)
    {
        if (currHp >= maxHp) return;

        currHp = Mathf.Min(currHp + amount, maxHp);
        UpdateHearts();
    }

    public void Damage(int amount)
    {
        currHp -= amount;
        UpdateHearts();

        if (currHp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        AudioManager.Instance.PlaySFX("Die", 0.7f);

        PlayerController[] players =
            FindObjectsByType<PlayerController>(FindObjectsSortMode.None);

        foreach (var p in players)
            p.DisableAllMovement(true);

        StartCoroutine(DelayRespawn());
    }

    private IEnumerator DelayRespawn()
    {
        yield return new WaitForSeconds(1f);

        FindFirstObjectByType<CheckPointManager>().RespawnPlayers();

        PlayerController[] players =
            FindObjectsByType<PlayerController>(FindObjectsSortMode.None);

        foreach (var p in players)
            p.DisableAllMovement(false);

        currHp = maxHp;
        UpdateHearts();
    }

    // ---------------- UI ----------------

    private void UpdateHearts()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i].enabled = i < currHp;
        }
    }
}
