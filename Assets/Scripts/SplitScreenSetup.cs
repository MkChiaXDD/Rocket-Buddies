using UnityEngine;
using UnityEngine.InputSystem;

public class SplitScreenSetup : MonoBehaviour
{
    [SerializeField] private GameObject DividerCannvas;

    private void Start()
    {
        if (DividerCannvas) DividerCannvas.SetActive(false);
    }

    private void OnPlayerJoined(PlayerInput player)
    {
        if (DividerCannvas) DividerCannvas.SetActive(true);

        Camera cam = player.GetComponentInChildren<Camera>();

        if (player.playerIndex == 0)
            cam.rect = new Rect(0f, 0f, 1f, 0.5f); // left half
        else if (player.playerIndex == 1)
            cam.rect = new Rect(0f, 0.5f, 1f, 0.5f); // right half
    }
}
