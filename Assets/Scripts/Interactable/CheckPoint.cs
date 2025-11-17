using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    [SerializeField] private int checkPointIndex;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            FindFirstObjectByType<CheckPointManager>().SetCheckPoint(checkPointIndex, transform.position);
        }
    }
}
