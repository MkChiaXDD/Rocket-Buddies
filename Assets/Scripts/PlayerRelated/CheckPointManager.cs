using UnityEngine;

public class CheckPointManager : MonoBehaviour
{
    [SerializeField] private int currCheckPointIndex = 0;
    private Vector3 LatestCheckPointLocation;

    private CheckPoint currentCheckPoint;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            RespawnPlayers();
        }
    }

    // Call this when a checkpoint is triggered
    public void SetCheckPoint(int index, Vector3 loc)
    {
        if (index > currCheckPointIndex)
        {
            currCheckPointIndex = index;
            LatestCheckPointLocation = loc;

            currentCheckPoint = FindCheckPointByIndex(index);
        }
    }

    public void RespawnPlayers()
    {
        // Find ALL players with the tag "Player"
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            // Reset position
            player.transform.position = LatestCheckPointLocation;

            // Optional: also reset velocity so they don’t fly away
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.linearVelocity = Vector2.zero;

            AudioManager.Instance.PlaySFX("Respawn", 0.7f);

            // Optional: disable/enable scripts for safety
            //player.GetComponent<PlayerController>().ResetState();
        }

        // ?? THIS IS THE KEY LINE
        if (currentCheckPoint != null)
            currentCheckPoint.ResetEnemies();
    }

    private CheckPoint FindCheckPointByIndex(int index)
    {
        CheckPoint[] cps = FindObjectsByType<CheckPoint>(FindObjectsSortMode.None);

        foreach (var cp in cps)
        {
            if (cp != null && cp.GetCheckPointIndex() == index)
                return cp;
        }

        return null;
    }
}
