using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] private bool canBeToggled = false;
    private bool activated = false;
    [SerializeField] private GameObject closedDoor;
    [SerializeField] private GameObject openDoor;
    private BoxCollider2D boxCollider;

    // set this in Inspector to only hit the Player layer
    [SerializeField] private LayerMask playerLayer;

    private void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        if (closedDoor) closedDoor.SetActive(true);
        if (openDoor) openDoor.SetActive(false);
    }

    public void SetDoor(bool open)
    {
        if (!closedDoor && !openDoor) return;

        if (!activated)
        {
            // if we are closing the door, check for players inside it
            if (!open)
            {
                CrushPlayersInside();
            }

            ToggleDoorCollider(open);
            ToggleDoorVisuals(open);
        }

        if (!canBeToggled) activated = true;
    }

    private void ToggleDoorVisuals(bool open)
    {
        openDoor.SetActive(open);
        closedDoor.SetActive(!open);
    }

    private void ToggleDoorCollider(bool open)
    {
        boxCollider.isTrigger = open;   // trigger when open, solid when closed
    }

    private void CrushPlayersInside()
    {
        // use the door collider’s bounds as the overlap area
        Bounds b = boxCollider.bounds;

        Collider2D[] hits = Physics2D.OverlapBoxAll(
            b.center,
            b.size,
            0f,
            playerLayer
        );

        foreach (var hit in hits)
        {
            // whatever script handles your death/respawn
            var health = hit.GetComponent<HealthManager>();   // or PlayerRespawn, etc.
            if (health != null)
            {
                health.Damage(99);   // or health.TakeDamage(999), health.Die(), etc.
            }
        }
    }

    // just to help visualize in editor
    private void OnDrawGizmosSelected()
    {
        if (!boxCollider) boxCollider = GetComponent<BoxCollider2D>();
        if (!boxCollider) return;

        Gizmos.color = Color.red;
        Bounds b = boxCollider.bounds;
        Gizmos.DrawWireCube(b.center, b.size);
    }
}
