using System.Collections.Generic;
using UnityEngine;

public class StepButton : MonoBehaviour
{
    private bool isActive;
    private int playersOnButton = 0;

    [SerializeField] private GameObject unPressedButton;
    [SerializeField] private GameObject pressedButton;

    [SerializeField] private List<Door> doors;

    private void Start()
    {
        isActive = false;
        playersOnButton = 0;

        if (unPressedButton) unPressedButton.SetActive(true);
        if (pressedButton) pressedButton.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        playersOnButton++;

        if (!isActive)        // only need to activate once
        {
            SetButton(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        playersOnButton = Mathf.Max(0, playersOnButton - 1);

        if (playersOnButton == 0)   // only deactivate when NO players left
        {
            SetButton(false);
        }
    }

    private void SetButton(bool pressed)
    {
        if (unPressedButton) unPressedButton.SetActive(!pressed);
        if (pressedButton) pressedButton.SetActive(pressed);
        isActive = pressed;
        OpenDoors();
    }

    private void OpenDoors()
    {
        foreach (var door in doors)
        {
            door.SetDoor(isActive);
        }
    }
}
