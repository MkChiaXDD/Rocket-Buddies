using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [SerializeField] private GameObject Divider;
    [SerializeField] private GameObject TextField;
    [SerializeField] private Transform[] spawnPoints;

    [Header("Cameras")]
    [SerializeField] private CameraFollow2D sharedCamera;
    [SerializeField] private CameraFollow2D[] playerCameras;


    [Header("Shared Camera Offset Control")]
    [SerializeField] private float offsetSmoothSpeed = 5f;

    private Vector2 baseOffset = Vector2.zero;
    private Vector2 currentOffset = Vector2.zero;
    private Vector2 targetOffset = Vector2.zero;

    [Header("Cinematic Split Settings")]
    [SerializeField] private float splitTransitionSpeed = 2.5f;
    [SerializeField] private AnimationCurve splitCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private float splitBlend = 0f; // 0 = shared, 1 = fully split
    private float targetBlend = 0f;


    [Header("Distances")]
    [SerializeField] private float splitDistance = 18f;
    [SerializeField] private float mergeDistance = 14f;

    [Header("Animator Controllers")]
    [SerializeField] private RuntimeAnimatorController player1AnimatorController;
    [SerializeField] private RuntimeAnimatorController player2AnimatorController;

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

        player.gameObject.name = (index == 0) ? "Player1" : "Player2";

        // ? Set the animator controller based on which player it is
        Transform capsule = player.transform.Find("Capsule");
        if (capsule != null)
        {
            Animator anim = capsule.GetComponent<Animator>();
            if (anim != null)
            {
                if (index == 0 && player1AnimatorController != null)
                {
                    anim.runtimeAnimatorController = player1AnimatorController;
                }
                else if (index == 1 && player2AnimatorController != null)
                {
                    anim.runtimeAnimatorController = player2AnimatorController;
                }
            }
        }

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

        Vector3 p1 = playerFollowTargets[0].position;
        Vector3 p2 = playerFollowTargets[1].position;

        Vector3 midpoint = (p1 + p2) * 0.5f;

        if (usingShared)
        {
            currentOffset = Vector2.Lerp(
                currentOffset,
                targetOffset,
                offsetSmoothSpeed * Time.deltaTime
            );

            midpoint.x += currentOffset.x;
            midpoint.y += currentOffset.y;
        }

        midTarget.position = midpoint;

        float dist = Vector3.Distance(p1, p2);

        if (usingShared && dist > splitDistance)
        {
            SwitchToSplit();
        }
        else if (!usingShared && dist < mergeDistance)
        {
            SwitchToShared();
        }

        splitBlend = Mathf.MoveTowards(
            splitBlend,
            targetBlend,
            splitTransitionSpeed * Time.deltaTime
        );

        float t = splitCurve.Evaluate(splitBlend);

        UpdateCameraRects(t);

        if (splitBlend <= 0.01f)
        {
            SetSplitCamerasActive(false);
            SetSharedCameraActive(true);
            Divider?.SetActive(false);
        }
        else
        {
            // active during transition AND full split
            Divider?.SetActive(true);

            if (splitBlend >= 0.99f)
            {
                SetSharedCameraActive(false);
                SetSplitCamerasActive(true);
            }
            else
            {
                SetSharedCameraActive(true);
                SetSplitCamerasActive(true);
            }
        }

    }


    private void SwitchToSplit()
    {
        usingShared = false;
        targetBlend = 1f;
        Divider?.SetActive(true);
    }

    private void SwitchToShared()
    {
        usingShared = true;
        targetBlend = 0f;
        Divider?.SetActive(true); // keep active during blend
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

    public void SetSharedCameraOffset(Vector2 offset)
    {
        targetOffset = offset;
    }

    public void ResetSharedCameraOffset()
    {
        targetOffset = baseOffset;
    }

    private void UpdateCameraRects(float t)
    {
        if (playerCameras.Length < 2) return;

        if (sharedCamera?.Cam != null)
        {
            sharedCamera.Cam.rect = new Rect(
                0f,
                Mathf.Lerp(0f, 0.25f, t),
                1f,
                Mathf.Lerp(1f, 0.5f, t)
            );
        }

        if (playerCameras[0]?.Cam != null)
        {
            playerCameras[0].Cam.rect = new Rect(
                0f,
                Mathf.Lerp(0.25f, 0.5f, t),
                1f,
                Mathf.Lerp(0.5f, 0.5f, t)
            );
        }

        if (playerCameras[1]?.Cam != null)
        {
            playerCameras[1].Cam.rect = new Rect(
                0f,
                Mathf.Lerp(0.25f, 0f, t),
                1f,
                Mathf.Lerp(0.5f, 0.5f, t)
            );
        }

        if (Divider != null)
        {
            Divider.transform.localScale = new Vector3(1f, Mathf.Lerp(0f, 1f, t), 1f);
        }
    }

}
