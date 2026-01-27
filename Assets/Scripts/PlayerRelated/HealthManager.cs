using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHp = 3;
    private int currHp;
    [SerializeField] private bool godMode = false;

    [Header("UI")]
    [SerializeField] private Image[] hearts;

    private bool alreadyDead = false;
    public bool IsDead => alreadyDead;

    private void Start()
    {
        currHp = maxHp;
        UpdateHearts();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            Damage(1);
        }
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
        if (godMode) return;
        if (alreadyDead) return;

        AudioManager.Instance.PlaySFX("Hit");

        currHp -= amount;
        UpdateHearts();

        if (currHp <= 0)
        {
            Die();
        }
    }


    private void Die()
    {
        if (alreadyDead) return; // SAFETY

        alreadyDead = true;

        AudioManager.Instance.PlaySFX("Die", 0.7f);

        FindFirstObjectByType<DeathCounter>()
            ?.IncreaseDeath(gameObject.name);

        PlayerController[] players =
            FindObjectsByType<PlayerController>(FindObjectsSortMode.None);

        foreach (var p in players)
            p.DisableAllMovement(true);

        StartCoroutine(DelayRespawn());
    }


    private IEnumerator DelayRespawn()
    {
        alreadyDead = true;
        yield return new WaitForSeconds(1f);

        FindFirstObjectByType<CheckPointManager>().RespawnPlayers();

        PlayerController[] players =
            FindObjectsByType<PlayerController>(FindObjectsSortMode.None);

        foreach (var p in players)
            p.DisableAllMovement(false);

        currHp = maxHp;
        alreadyDead = false;
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
