using UnityEngine;
using UnityEngine.InputSystem;

public class SplitScreenSetup : MonoBehaviour
{
    private void OnPlayerJoined(PlayerInput player)
    {
        Camera cam = player.GetComponentInChildren<Camera>();

        if (player.playerIndex == 0)
            cam.rect = new Rect(0f, 0f, 1f, 0.5f); // left half
        else if (player.playerIndex == 1)
            cam.rect = new Rect(0f, 0.5f, 1f, 0.5f); // right half
    }
}
