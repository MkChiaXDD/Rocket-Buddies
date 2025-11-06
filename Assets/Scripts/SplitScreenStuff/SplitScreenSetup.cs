using UnityEngine;
using UnityEngine.InputSystem;

public class SplitScreenSetup : MonoBehaviour
{
    [SerializeField] private GameObject Divider;
    [SerializeField] private GameObject TextField;
    [SerializeField] private Transform[] spawnPoints;
    private int playerCount = 0;

    private void Start()
    {
        if (Divider) Divider.SetActive(false);
        if (TextField) TextField.SetActive(true);
    }

    private void OnPlayerJoined(PlayerInput player)
    {
        playerCount++;

        // === ?? Set player spawn position ===
        if (spawnPoints != null && player.playerIndex < spawnPoints.Length)
        {
            player.transform.position = spawnPoints[player.playerIndex].position;
        }

        // Wait until both players have joined
        if (playerCount < 2)
            return;

        if (TextField) TextField.SetActive(false);
        if (Divider) Divider.SetActive(true);

        // Get both players and assign camera rects
        var players = Object.FindObjectsByType<PlayerInput>(FindObjectsSortMode.None);
        foreach (var p in players)
        {
            Camera cam = p.GetComponentInChildren<Camera>();
            if (!cam) continue;

            if (p.playerIndex == 0)
                cam.rect = new Rect(0f, 0.5f, 1f, 0.5f); // top half
            else if (p.playerIndex == 1)
                cam.rect = new Rect(0f, 0f, 1f, 0.5f); // bottom half
        }
    }
}
