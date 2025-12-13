using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    [SerializeField] private int checkPointIndex;

    [Header("Visual Indicators")]
    [SerializeField] private GameObject inactiveIndicator; // shown before activation
    [SerializeField] private GameObject activeIndicator;   // shown after activation

    private bool player1Reached = false;
    private bool player2Reached = false;
    private bool activated = false;

    private void Start()
    {
        // Initial visual state
        if (inactiveIndicator) inactiveIndicator.SetActive(true);
        if (activeIndicator) activeIndicator.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (activated)
            return; // already activated, nothing else needed

        // Identify which player entered
        if (collision.gameObject.name == "Player1")
        {
            player1Reached = true;
        }
        else if (collision.gameObject.name == "Player2")
        {
            player2Reached = true;
        }
        else
        {
            return; // not a player
        }

        // Only activate when BOTH have arrived
        if (player1Reached && player2Reached)
        {
            activated = true;

            // Notify manager
            FindFirstObjectByType<CheckPointManager>()
                .SetCheckPoint(checkPointIndex, transform.position);

            AudioManager.Instance.PlaySFX("Checkpoint");

            // Switch visuals
            if (inactiveIndicator) inactiveIndicator.SetActive(false);
            if (activeIndicator) activeIndicator.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Identify which player entered
        if (collision.gameObject.name == "Player1")
        {
            player1Reached = false;
        }
        else if (collision.gameObject.name == "Player2")
        {
            player2Reached = false;
        }
        else
        {
            return; // not a player
        }
    }
}
