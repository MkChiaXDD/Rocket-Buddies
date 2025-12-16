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

    [Header("Endpoint Behaviour")]
    [SerializeField] private bool goBack = true; // NEW

    private float upTimer = 0f;
    private float downTimer = 0f;

    private bool movingUp = false;
    private bool reachedEndPoint = false; // NEW

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // If endpoint reached and goBack is OFF, lock platform at top
        if (reachedEndPoint && !goBack)
            return;

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
            // Players left ? prepare to move DOWN
            downTimer += Time.deltaTime;
            upTimer = 0f;

            if (downTimer >= moveDownDelay)
                movingUp = false;
        }

        Vector3 target = movingUp ? endPoint.position : startPos;
        float finalMoveSpeed = 0f;
        if (!movingUp)
        {
            finalMoveSpeed = moveSpeed * 2f;
        }
        else
        {
            finalMoveSpeed = moveSpeed;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            finalMoveSpeed * Time.deltaTime
        );

        // Check if endpoint is reached
        if (!reachedEndPoint && Vector3.Distance(transform.position, endPoint.position) < 0.01f)
        {
            reachedEndPoint = true;
        }
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
