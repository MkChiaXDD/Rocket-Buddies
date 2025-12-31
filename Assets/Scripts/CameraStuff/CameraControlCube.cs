using UnityEngine;

public class CameraControlCube : MonoBehaviour
{
    [Header("Camera Mode To Switch To")]
    [SerializeField] private CameraController.CameraMode cameraMode;

    [Header("Optional Target (for SharedWithTarget)")]
    [SerializeField] private Transform sharedTarget;

    private CameraController cameraController;

    private void Awake()
    {
        cameraController = FindFirstObjectByType<CameraController>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        if (cameraController == null)
            return;

        switch (cameraMode)
        {
            case CameraController.CameraMode.Shared:
                cameraController.SetShared();
                break;

            case CameraController.CameraMode.Split:
                cameraController.SetSplit();
                break;

            case CameraController.CameraMode.SharedWithTarget:
                if (sharedTarget != null)
                    cameraController.SetSharedWithTarget(sharedTarget);
                else
                    Debug.LogWarning("SharedWithTarget selected but no target assigned.", this);
                break;
        }
    }
}
