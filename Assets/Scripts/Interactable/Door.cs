using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] private bool canBeToggled;
    [SerializeField] private GameObject closedDoor;
    [SerializeField] private GameObject openDoor;

    private void Start()
    {
        if (closedDoor) closedDoor.SetActive(true);
        if (openDoor) openDoor.SetActive(false);
    }

    private void SetDoor(bool open)
    {
        if (closedDoor) closedDoor.SetActive(!open);
        if (openDoor) openDoor.SetActive(open);
    }
}
