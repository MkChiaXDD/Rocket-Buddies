using UnityEngine;
using TMPro;

public class Door : MonoBehaviour
{
    [SerializeField] private bool canBeToggled = false;
    private bool activated = false;

    [SerializeField] private GameObject closedDoor;
    [SerializeField] private GameObject openDoor;
    [SerializeField] private TMP_Text timerText;
    private float timerRemaining = 0f;
    private bool timerRunning = false;

    private BoxCollider2D boxCollider;

    // how many players are inside the trigger
    private int playersInside = 0;

    private HealthManager hpMgr;

    private void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        hpMgr = FindFirstObjectByType<HealthManager>();

        if (closedDoor) closedDoor.SetActive(true);
        if (openDoor) openDoor.SetActive(false);
    }

    public void SetDoor(bool open)
    {
        if (!closedDoor && !openDoor) return;

        // if we are CLOSING the door and at least one player is inside ? kill
        if (!open && playersInside > 0 && hpMgr != null)
        {
            hpMgr.Damage(99);   // shared health, so damage once
        }

        if (!activated)
        {
            ToggleDoorCollider(open);
            ToggleDoorVisuals(open);
        }

        if (!canBeToggled)
            activated = true;
    }

    private void ToggleDoorVisuals(bool open)
    {
        if (openDoor) openDoor.SetActive(open);
        if (closedDoor) closedDoor.SetActive(!open);
    }

    private void ToggleDoorCollider(bool open)
    {
        if (boxCollider)
            boxCollider.isTrigger = open;   // trigger when open, solid when closed
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playersInside++;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playersInside = Mathf.Max(0, playersInside - 1);
        }
    }

    public void StartTimer(float duration)
    {
        timerRemaining = Mathf.Max(0f, duration);
        timerRunning = true;
        UpdateTimerText();
    }

    private void UpdateTimerText()
    {
        if (timerText == null) return;

        if (!timerRunning)
        {
            timerText.text = "";
            return;
        }

        timerText.text = timerRemaining.ToString("F2");
    }

    private void Update()
    {
        if (!timerRunning) return;

        timerRemaining -= Time.deltaTime;

        if (timerRemaining <= 0f)
        {
            timerRemaining = 0f;
            timerRunning = false;
            timerText.text = "";
        }

        UpdateTimerText();
    }
}
