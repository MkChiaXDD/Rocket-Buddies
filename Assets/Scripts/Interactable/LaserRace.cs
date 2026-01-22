using UnityEngine;

public class LaserRace : MonoBehaviour
{
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;
    [SerializeField] private GameObject itemToMove;
    [SerializeField] private float moveSpeed = 5f;

    private bool isMoving = false;

    private void Start()
    {
        ResetRace();
    }

    private void Update()
    {
        if (!isMoving || itemToMove == null) return;

        MoveTowardsEnd();
    }

    private void MoveTowardsEnd()
    {
        Vector3 targetPos = endPoint.position;

        itemToMove.transform.position = Vector3.MoveTowards(
            itemToMove.transform.position,
            targetPos,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(itemToMove.transform.position, targetPos) <= 0.01f)
        {
            isMoving = false;
            OnRaceFinished();
        }
    }

    public void StartRace()
    {
        itemToMove.SetActive(true);
        isMoving = true;
    }

    public void StopRace()
    {
        isMoving = false;
    }

    public void ResetRace()
    {
        if (itemToMove != null && startPoint != null)
        {
            Debug.Log("Race Resetted");
            itemToMove.transform.position = startPoint.position;
            itemToMove.SetActive(false);
        }
    }

    private void OnRaceFinished()
    {
        Debug.Log("Laser reached the end!");
        // Kill players, trigger checkpoint, etc
    }
}
