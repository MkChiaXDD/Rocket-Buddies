using System.Collections;
using UnityEngine;

public class HealthManager : MonoBehaviour
{
    [SerializeField] private int maxHp = 1;
    private int currHp;

    private void Start()
    {
        currHp = maxHp;
    }

    public void Heal(int amount)
    {
        if (currHp >= maxHp) return;

        currHp += amount;
    }

    public void Damage(int amount)
    {
        currHp -= amount;

        if (currHp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        currHp = maxHp;

        AudioManager.Instance.PlaySFX("Die", 0.7f);

        PlayerController[] players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        foreach (var p in players)
        {
            p.DisableAllMovement(true);
        }

        StartCoroutine(DelayRespawn());
    }

    private IEnumerator DelayRespawn()
    {
        yield return new WaitForSeconds(1f);

        //Call respawn function
        FindFirstObjectByType<CheckPointManager>().RespawnPlayers();

        PlayerController[] players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        foreach (var p in players)
        {
            p.DisableAllMovement(false);
        }
    }
}
