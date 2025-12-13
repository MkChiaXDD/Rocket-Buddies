using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    private bool player1Active = false;
    private bool player2Active = false;

    private Vector3 startPos;
    [SerializeField] private Transform endPoint;
    [SerializeField] private float moveSpeed = 2f;

    [Header("Delays")]
    [SerializeField] private float moveUpDelay = 1f;
    [SerializeField] private float moveDownDelay = 2f;

    private float upTimer = 0f;
    private float downTimer = 0f;

    private bool movingUp = false;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // BOTH players on platform ? prepare to move UP
        if (player1Active && player2Active)
        {
            upTimer += Time.deltaTime;
            downTimer = 0f;

            if (upTimer >= moveUpDelay)
                movingUp = true;
        }
        else
        {
            // One or both players left ? prepare to move DOWN
            downTimer += Time.deltaTime;
            upTimer = 0f;

            if (downTimer >= moveDownDelay)
                movingUp = false;
        }

        // Move platform
        Vector3 target;

        if (movingUp)
            target = endPoint.position;
        else
            target = startPos;

        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            moveSpeed * Time.deltaTime
        );
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.name == "Player1")
        {
            player1Active = true;
            col.transform.SetParent(transform);
        }
        else if (col.gameObject.name == "Player2")
        {
            player2Active = true;
            col.transform.SetParent(transform);
        }
    }

    private void OnCollisionExit2D(Collision2D col)
    {
        if (col.gameObject.name == "Player1")
        {
            player1Active = false;
            col.transform.SetParent(null);
        }
        else if (col.gameObject.name == "Player2")
        {
            player2Active = false;
            col.transform.SetParent(null);
        }
    }
}
