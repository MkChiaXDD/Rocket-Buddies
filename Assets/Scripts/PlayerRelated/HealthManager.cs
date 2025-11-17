using Mono.Cecil.Cil;
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

        //Call respawn function
        FindFirstObjectByType<CheckPointManager>().RespawnPlayers();
    }
}
