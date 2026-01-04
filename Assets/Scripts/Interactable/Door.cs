using UnityEngine;
using TMPro;

public class Door : MonoBehaviour
{
    public enum DoorType
    {
        Normal,
        EnemyLocked,
    }

    [Header("Door Type")]
    [SerializeField] private DoorType doorType = DoorType.Normal;

    [Header("Door Visuals")]
    [SerializeField] private GameObject closedDoor;
    [SerializeField] private GameObject openDoor;

    [Header("Timer (Optional)")]
    [SerializeField] private TMP_Text timerText;
    private float timerRemaining;
    private bool timerRunning;

    private BoxCollider2D boxCollider;

    // how many players are inside the door trigger
    private int playersInside;

    private HealthManager hpMgr;

    // ---------- Enemy Unlock ----------
    [Header("Enemy Unlock Settings")]
    [SerializeField] private EnemyBase[] requiredEnemies;
    private int enemiesRemaining;

    // ---------------- UNITY ----------------

    private void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        hpMgr = FindFirstObjectByType<HealthManager>();

        CloseDoorImmediate();

        if (doorType == DoorType.EnemyLocked)
            SetupEnemyGate();
    }

    // ---------------- SETUP ----------------

    private void SetupEnemyGate()
    {
        if (requiredEnemies == null || requiredEnemies.Length == 0)
        {
            Debug.LogWarning($"{name}: EnemyLocked door has no enemies assigned.");
            return;
        }

        enemiesRemaining = requiredEnemies.Length;

        foreach (var enemy in requiredEnemies)
        {
            if (enemy != null)
                enemy.OnEnemyDied += HandleEnemyKilled;
        }
    }

    // ---------------- DOOR CONTROL ----------------

    public void OpenDoor()
    {
        ToggleDoor(true);
        AudioManager.Instance.PlaySFX("Door");
    }

    public void CloseDoor()
    {
        ToggleDoor(false);
        AudioManager.Instance.PlaySFX("Door");
    }

    private void ToggleDoor(bool open)
    {
        // Killing players if door closes on them
        if (!open && playersInside > 0 && hpMgr != null)
        {
            hpMgr.Damage(99);
            playersInside = 0;
        }

        ToggleDoorVisuals(open);
        ToggleDoorCollider(open);
    }

    private void CloseDoorImmediate()
    {
        ToggleDoorVisuals(false);
        ToggleDoorCollider(false);
    }

    private void ToggleDoorVisuals(bool open)
    {
        if (openDoor) openDoor.SetActive(open);
        if (closedDoor) closedDoor.SetActive(!open);
    }

    private void ToggleDoorCollider(bool open)
    {
        if (boxCollider)
            boxCollider.isTrigger = open;
    }

    // ---------------- PLAYER TRACKING ----------------

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            playersInside++;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            playersInside = Mathf.Max(0, playersInside - 1);
    }

    // ---------------- TIMER ----------------

    public void StartTimer(float duration)
    {
        if (doorType != DoorType.Normal) return;

        timerRemaining = Mathf.Max(0f, duration);
        timerRunning = true;
        OpenDoor();
        UpdateTimerText();
    }

    private void Update()
    {
        if (!timerRunning) return;

        timerRemaining -= Time.deltaTime;

        if (timerRemaining <= 0f)
        {
            timerRemaining = 0f;
            timerRunning = false;
            CloseDoor();
            ClearTimerText();
        }

        UpdateTimerText();
    }

    private void UpdateTimerText()
    {
        if (timerText != null)
            timerText.text = timerRemaining.ToString("F2");
    }

    private void ClearTimerText()
    {
        if (timerText != null)
            timerText.text = "";
    }

    // ---------------- ENEMY UNLOCK ----------------

    private void HandleEnemyKilled(EnemyBase enemy)
    {
        if (doorType != DoorType.EnemyLocked) return;

        enemiesRemaining--;

        if (enemiesRemaining <= 0)
        {
            OpenDoor();
        }
    }

    public void ResetEnemyGate()
    {
        if (doorType != DoorType.EnemyLocked)
            return;

        enemiesRemaining = 0;

        if (requiredEnemies == null)
            return;

        // Re-count enemies (only alive ones)
        foreach (var enemy in requiredEnemies)
        {
            if (enemy == null) continue;

            enemiesRemaining++;

            // avoid duplicate subscriptions
            enemy.OnEnemyDied -= HandleEnemyKilled;
            enemy.OnEnemyDied += HandleEnemyKilled;
        }

        CloseDoor();
    }

}
