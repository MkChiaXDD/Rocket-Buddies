using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class StepButton : MonoBehaviour
{
    private bool isActive;
    [SerializeField] private GameObject unPressedButton;
    [SerializeField] private GameObject pressedButton;

    [SerializeField] private List<Door> doors;

    private void Start()
    {
        isActive = false;
        if (unPressedButton) unPressedButton.SetActive(true);
        if (pressedButton) pressedButton.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            SetButton(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
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
            if (isActive)
            {
                door.SetDoor(true);
            }
            else
            {
                door.SetDoor(false);
            }
        }
    }

}
