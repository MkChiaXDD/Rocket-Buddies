using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    [SerializeField] private Transform target; // your player
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private Vector3 offset;   // e.g. new Vector3(0, 1, -10)

    void LateUpdate()
    {
        if (!target) return;

        Vector3 desired = target.position + offset;
        Vector3 smoothed = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);
        transform.position = smoothed;

        // keep camera rotation fixed (no rotation inheritance)
        transform.rotation = Quaternion.identity;
    }
}
