using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    [SerializeField] private int checkPointIndex;

    [Header("Visual Indicators")]
    [SerializeField] private GameObject inactiveIndicator; // shown before activation
    [SerializeField] private GameObject activeIndicator;   // shown after activation

    private bool player1Reached = false;
    private bool player2Reached = false;
    private bool activated = false;

    [Header("Enemies for this Checkpoint")]
    [SerializeField] private EnemyBase[] checkpointEnemies;

    [Header("Boss Reset")]
    [SerializeField] private BossArenaManager bossArena;

    [Header("Door To Unlock")]
    [SerializeField] private Door linkedDoor;   // DRAG DOOR HERE ??
    [SerializeField] private Door enemyResetDoor;

    [Header("Laser Race")]
    [SerializeField] private LaserRace race;

    private void Start()
    {
        // Initial visual state
        if (inactiveIndicator) inactiveIndicator.SetActive(true);
        if (activeIndicator) activeIndicator.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (activated)
            return;

        // Identify which player entered
        if (collision.gameObject.name == "Player1")
        {
            player1Reached = true;
        }
        else if (collision.gameObject.name == "Player2")
        {
            player2Reached = true;
        }
        else
        {
            return;
        }

        // Only activate when BOTH have arrived
        if (player1Reached && player2Reached)
        {
            activated = true;

            // Notify manager
            FindFirstObjectByType<CheckPointManager>()
                .SetCheckPoint(checkPointIndex, transform.position);

            AudioManager.Instance.PlaySFX("Checkpoint");

            // Switch visuals
            if (inactiveIndicator) inactiveIndicator.SetActive(false);
            if (activeIndicator) activeIndicator.SetActive(true);

            // ?? OPEN LINKED DOOR
            if (linkedDoor != null)
            {
                linkedDoor.OpenDoor();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.name == "Player1")
        {
            player1Reached = false;
        }
        else if (collision.gameObject.name == "Player2")
        {
            player2Reached = false;
        }
    }

    public void ResetEnemies()
    {
        if (checkpointEnemies == null) return;

        foreach (var enemy in checkpointEnemies)
        {
            if (enemy != null)
                enemy.ResetEnemy();
        }

        if (enemyResetDoor != null)
        {
            enemyResetDoor.ResetEnemyGate();
        }
    }

    public void ResetBoss()
    {
        if (bossArena == null) return;

        bossArena.Reset();
    }

    public int GetCheckPointIndex()
    {
        return checkPointIndex;
    }

    public bool IsActivated()
    {
        return activated;
    }

    public void ResetRace()
    {
        if (race == null) return;

        race.ResetRace();
    }
}
