using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] private bool canBeToggled = false;
    private bool activated = false;

    [SerializeField] private GameObject closedDoor;
    [SerializeField] private GameObject openDoor;

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
}
