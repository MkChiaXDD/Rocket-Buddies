using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    // ================= ENUM =================
    public enum CameraMode
    {
        Shared,
        SharedWithTarget,
        Split
    }

    [Header("Current Mode")]
    [SerializeField] private CameraMode currentMode = CameraMode.Shared;

    // ================= REFERENCES =================
    [Header("UI")]
    [SerializeField] private GameObject divider;
    [SerializeField] private GameObject textField;

    [Header("Spawn")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Cameras")]
    [SerializeField] private CameraFollow2D sharedCamera;
    [SerializeField] private CameraFollow2D[] playerCameras;

    // ================= ANIMATION =================
    [Header("Animator Controllers")]
    [SerializeField] private RuntimeAnimatorController player1AnimatorController;
    [SerializeField] private RuntimeAnimatorController player2AnimatorController;

    // ================= PLAYERS =================
    private Transform[] playerTargets = new Transform[2];
    private int playerCount;

    // ================= SHARED CAMERA =================
    private Transform midTarget;
    private Transform extraTarget; // for SharedWithTarget

    private Coroutine sharedShakeRoutine;
    private Vector3 sharedCamBaseLocalPos;

    private Coroutine sharedShakeLoop;
    private Vector3 sharedShakeOffset;

    // ================= UNITY =================
    private void Awake()
    {
        midTarget = new GameObject("CameraMidTarget").transform;
    }

    private void Start()
    {
        divider?.SetActive(false);
        textField?.SetActive(true);
    }

    // ================= PLAYER JOIN =================
    private void OnPlayerJoined(PlayerInput player)
    {
        int index = player.playerIndex;
        playerCount++;

        player.gameObject.name = index == 0 ? "Player1" : "Player2";

        // Spawn
        if (spawnPoints != null && index < spawnPoints.Length)
            player.transform.position = spawnPoints[index].position;

        // ---------- ANIMATOR SETUP (RESTORED) ----------
        Transform capsule = player.transform.Find("Capsule");
        if (capsule != null)
        {
            Animator anim = capsule.GetComponent<Animator>();
            if (anim != null)
            {
                if (index == 0 && player1AnimatorController != null)
                    anim.runtimeAnimatorController = player1AnimatorController;
                else if (index == 1 && player2AnimatorController != null)
                    anim.runtimeAnimatorController = player2AnimatorController;
            }
        }

        // Find FirePoint or fallback to player
        Transform followTarget = player.transform;
        foreach (Transform t in player.GetComponentsInChildren<Transform>(true))
        {
            if (t.name == "FirePoint")
            {
                followTarget = t;
                break;
            }
        }

        playerTargets[index] = followTarget;

        // Assign split camera
        if (index < playerCameras.Length && playerCameras[index] != null)
        {
            playerCameras[index].SetTarget(followTarget);

            if (playerCameras[index].Cam != null)
            {
                playerCameras[index].Cam.rect =
                    index == 0
                    ? new Rect(0f, 0.5f, 1f, 0.5f)
                    : new Rect(0f, 0f, 1f, 0.5f);
            }
        }

        // Both players ready
        if (playerCount == 2)
        {
            sharedCamera.SetTarget(midTarget);
            sharedCamera.Cam.rect = new Rect(0f, 0f, 1f, 1f);

            textField?.SetActive(false);
            SetMode(currentMode);
            FindFirstObjectByType<WorldManager>()?.StartGame();
        }
    }

    // ================= CAMERA UPDATE =================
    private void LateUpdate()
    {
        if (playerCount < 2) return;
        if (currentMode == CameraMode.Split) return;

        Vector3 p1 = playerTargets[0].position;
        Vector3 p2 = playerTargets[1].position;

        Vector3 midpoint = (p1 + p2) * 0.5f;

        if (currentMode == CameraMode.SharedWithTarget && extraTarget != null)
        {
            midpoint = (midpoint * 2f + extraTarget.position) / 3f;
        }

        midTarget.position = midpoint;
    }

    // ================= PUBLIC API =================

    public void SetShared()
    {
        extraTarget = null;
        SetMode(CameraMode.Shared);
    }

    public void SetSharedWithTarget(Transform target)
    {
        extraTarget = target;
        SetMode(CameraMode.SharedWithTarget);
    }

    public void SetSplit()
    {
        extraTarget = null;
        SetMode(CameraMode.Split);
    }

    // ================= MODE HANDLER =================
    private void SetMode(CameraMode mode)
    {
        currentMode = mode;

        switch (currentMode)
        {
            case CameraMode.Shared:
            case CameraMode.SharedWithTarget:
                ActivateShared();
                break;

            case CameraMode.Split:
                ActivateSplit();
                break;
        }
    }

    // ================= MODE ACTIVATION =================
    private void ActivateShared()
    {
        sharedCamera.Cam.enabled = true;
        SetSplitCameras(false);
        divider?.SetActive(false);
    }

    private void ActivateSplit()
    {
        sharedCamera.Cam.enabled = false;
        SetSplitCameras(true);
        divider?.SetActive(true);
    }

    private void SetSplitCameras(bool active)
    {
        foreach (var cam in playerCameras)
        {
            if (cam && cam.Cam)
                cam.Cam.enabled = active;
        }
    }

    public void ShakeSharedCamera(float strength, float duration)
    {
        if (sharedCamera == null || sharedCamera.Cam == null) return;
        if (!sharedCamera.Cam.enabled) return;          // only shake when shared cam is active
        if (strength <= 0f || duration <= 0f) return;

        if (sharedShakeRoutine != null)
            StopCoroutine(sharedShakeRoutine);

        sharedShakeRoutine = StartCoroutine(ShakeSharedRoutine(strength, duration));
    }

    private IEnumerator ShakeSharedRoutine(float strength, float duration)
    {
        Transform camT = sharedCamera.Cam.transform;

        sharedShakeOffset = Vector3.zero;

        float t = 0f;
        while (t < duration)
        {
            yield return new WaitForEndOfFrame(); // let camera follow move first

            // remove last offset
            camT.localPosition -= sharedShakeOffset;

            // apply new offset
            sharedShakeOffset = new Vector3(
                Random.Range(-1f, 1f) * strength,
                Random.Range(-1f, 1f) * strength,
                0f
            );

            camT.localPosition += sharedShakeOffset;

            t += Time.deltaTime;
        }

        // clean up
        camT.localPosition -= sharedShakeOffset;
        sharedShakeOffset = Vector3.zero;
        sharedShakeRoutine = null;
    }


    public void StartSharedShake(float strength)
    {
        if (sharedCamera == null || sharedCamera.Cam == null) return;
        if (!sharedCamera.Cam.enabled) return;
        if (strength <= 0f) return;

        if (sharedShakeLoop != null) return;
        sharedShakeOffset = Vector3.zero;
        sharedShakeLoop = StartCoroutine(SharedShakeLoop(strength));
    }

    public void StopSharedShake()
    {
        if (sharedCamera == null || sharedCamera.Cam == null) return;

        if (sharedShakeLoop != null)
            StopCoroutine(sharedShakeLoop);

        sharedShakeLoop = null;

        // remove current offset only (don’t reset camera position)
        sharedCamera.Cam.transform.localPosition -= sharedShakeOffset;
        sharedShakeOffset = Vector3.zero;
    }

    private IEnumerator SharedShakeLoop(float strength)
    {
        Transform camT = sharedCamera.Cam.transform;

        while (true)
        {
            yield return new WaitForEndOfFrame();

            camT.localPosition -= sharedShakeOffset;

            sharedShakeOffset = new Vector3(
                Random.Range(-1f, 1f) * strength,
                Random.Range(-1f, 1f) * strength,
                0f
            );

            camT.localPosition += sharedShakeOffset;
        }
    }
}
