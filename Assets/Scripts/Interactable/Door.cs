using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] private bool canBeToggled = false;
    private bool activated = false;
    [SerializeField] private GameObject closedDoor;
    [SerializeField] private GameObject openDoor;
    private BoxCollider2D boxCollider;

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
        boxCollider.isTrigger = open;
    }
}
