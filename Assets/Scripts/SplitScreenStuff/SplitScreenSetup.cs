using UnityEngine;
using UnityEngine.InputSystem;

public class SplitScreenSetup : MonoBehaviour
{
    [SerializeField] private GameObject Divider;
    [SerializeField] private GameObject TextField;
    [SerializeField] private Transform[] spawnPoints;

    [Header("Cameras")]
    [SerializeField] private CameraFollow2D sharedCamera;
    [SerializeField] private CameraFollow2D[] playerCameras; // size 2

    [Header("Distances")]
    [SerializeField] private float splitDistance = 18f; // when to split
    [SerializeField] private float mergeDistance = 14f; // when to merge back (smaller to avoid flicker)

    private int playerCount = 0;
    private Transform[] playerFollowTargets = new Transform[2];
    private Transform midTarget;            // midpoint object
    private bool usingShared = true;

    private void Awake()
    {
        // midpoint object for shared camera
        midTarget = new GameObject("CameraMidTarget").transform;
    }

    private void Start()
    {
        if (Divider) Divider.SetActive(false);
        if (TextField) TextField.SetActive(true);

        // start in shared mode (even if it won't show until 2 players)
        SetSharedCameraActive(true);
        SetSplitCamerasActive(false);
    }

    private void OnPlayerJoined(PlayerInput player)
    {
        int index = player.playerIndex;
        playerCount++;

        // Spawn position
        if (spawnPoints != null && index < spawnPoints.Length)
        {
            player.transform.position = spawnPoints[index].position;
        }

        // Find "FirePoint" anywhere under this player (not just direct child)
        Transform followTarget = player.transform; // default to player

        foreach (Transform t in player.GetComponentsInChildren<Transform>(true))
        {
            if (t.name == "FirePoint")
            {
                followTarget = t;
                break;
            }
        }

        playerFollowTargets[index] = followTarget;


        // Hook up player camera for this player
        if (index < playerCameras.Length && playerCameras[index] != null)
        {
            playerCameras[index].SetTarget(followTarget);

            if (playerCameras[index].Cam != null)
            {
                if (index == 0)
                    playerCameras[index].Cam.rect = new Rect(0f, 0.5f, 1f, 0.5f); // top
                else if (index == 1)
                    playerCameras[index].Cam.rect = new Rect(0f, 0f, 1f, 0.5f);   // bottom
            }
        }

        // When both players are in, enable cameras properly
        if (playerCount == 2)
        {
            // shared camera follows midpoint
            sharedCamera.SetTarget(midTarget);
            if (sharedCamera.Cam != null)
                sharedCamera.Cam.rect = new Rect(0f, 0f, 1f, 1f);

            if (TextField) TextField.SetActive(false);
            Divider?.SetActive(false); // start in single-camera mode

            FindFirstObjectByType<WorldOneManager>().StartGame();
        }
    }

    private void Update()
    {
        if (playerCount < 2) return;

        // update midpoint for shared camera
        Vector3 p1 = playerFollowTargets[0].position;
        Vector3 p2 = playerFollowTargets[1].position;
        midTarget.position = (p1 + p2) * 0.5f;

        float dist = Vector3.Distance(p1, p2);

        if (usingShared && dist > splitDistance)
        {
            SwitchToSplit();
        }
        else if (!usingShared && dist < mergeDistance)
        {
            SwitchToShared();
        }
    }

    private void SwitchToSplit()
    {
        usingShared = false;
        SetSharedCameraActive(false);
        SetSplitCamerasActive(true);
        Divider?.SetActive(true);
    }

    private void SwitchToShared()
    {
        usingShared = true;
        SetSplitCamerasActive(false);
        SetSharedCameraActive(true);
        Divider?.SetActive(false);
    }

    private void SetSharedCameraActive(bool active)
    {
        if (sharedCamera && sharedCamera.Cam)
            sharedCamera.Cam.enabled = active;
    }

    private void SetSplitCamerasActive(bool active)
    {
        foreach (var camFollow in playerCameras)
        {
            if (camFollow && camFollow.Cam)
                camFollow.Cam.enabled = active;
        }
    }
}
