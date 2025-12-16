using System.Collections;
using UnityEngine;

public class CameraControlCube : MonoBehaviour
{
    [SerializeField] private Vector2 cameraOffset = new Vector2(0 ,0);
    [SerializeField] private float duration = 3f;
    [SerializeField] private CameraController camCon;
    private bool isActive = false;

    private void Start()
    {
        if (!camCon)
        {
            camCon = FindFirstObjectByType<CameraController>();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isActive) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            if (duration > 0)
            {
                StartCoroutine(ResetTimer());
            }
            else
            {
                camCon.SetSharedCameraOffset(cameraOffset);
            }
        }
        
    }

    private IEnumerator ResetTimer()
    {
        isActive = true;
        camCon.SetSharedCameraOffset(cameraOffset);

        yield return new WaitForSeconds(duration);

        isActive = false;
        camCon.ResetSharedCameraOffset();
    }
}
