using UnityEngine;

public class CheckpointPortal : MonoBehaviour
{
    [SerializeField] private CheckPoint linkedCheckpoint;
    [SerializeField] private bool IsExitPortal = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (linkedCheckpoint == null) return;
        if (IsExitPortal) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            FloatingText.Instance.Show("World Two");
            collision.transform.position = linkedCheckpoint.transform.position;
        }
    }
}
