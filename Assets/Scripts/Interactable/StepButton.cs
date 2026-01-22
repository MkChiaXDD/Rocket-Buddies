using System.Collections.Generic;
using UnityEngine;

public class StepButton : MonoBehaviour
{
    private bool isActive;
    private int playersOnButton = 0;

    [Header("Visuals")]
    [SerializeField] private GameObject unPressedButton;
    [SerializeField] private GameObject pressedButton;

    [Header("Door Settings")]
    [SerializeField] private List<Door> doors;
    [SerializeField] private bool openRandomDoor = false;
    [SerializeField] private int requiredPlayers = 1; // 1 or 2 players needed

    private Door currentRandomDoor;

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

        if (!isActive && playersOnButton >= requiredPlayers)
            SetButton(true);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        playersOnButton = Mathf.Max(0, playersOnButton - 1);

        if (isActive && playersOnButton < requiredPlayers)
            SetButton(false);
    }

    private void SetButton(bool pressed)
    {
        isActive = pressed;

        if (unPressedButton) unPressedButton.SetActive(!pressed);
        if (pressedButton) pressedButton.SetActive(pressed);

        AudioManager.Instance.PlaySFX("ButtonClick");

        UpdateDoors();
    }

    private void UpdateDoors()
    {
        if (doors == null || doors.Count == 0) return;

        if (openRandomDoor)
        {
            HandleRandomDoor();
        }
        else
        {
            foreach (var door in doors)
            {
                if (isActive)
                    door.OpenDoor();
                else
                    door.CloseDoor();
            }
        }
    }

    private void HandleRandomDoor()
    {
        if (isActive)
        {
            int randomIndex = Random.Range(0, doors.Count);
            currentRandomDoor = doors[randomIndex];

            currentRandomDoor?.OpenDoor();
        }
        else
        {
            currentRandomDoor?.CloseDoor();
            currentRandomDoor = null;
        }
    }
}
