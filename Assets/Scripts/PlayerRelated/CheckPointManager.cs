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
        
        if (Input.GetKeyDown(KeyCode.O))
        {
            TeleportToNextCheckpoint();
        }
    }

    private void TeleportToNextCheckpoint()
    {
        int nextIndex = currCheckPointIndex + 1;
        string nextCheckpointName = "CheckPoint " + nextIndex;

        GameObject checkpointObj = GameObject.Find(nextCheckpointName);

        if (checkpointObj == null)
        {
            Debug.Log("No checkpoint found with name: " + nextCheckpointName);
            return;
        }

        CheckPoint nextCheckpoint = checkpointObj.GetComponent<CheckPoint>();

        if (nextCheckpoint == null)
        {
            Debug.Log("Checkpoint object has no CheckPoint component.");
            return;
        }

        Vector3 targetPos = checkpointObj.transform.position;

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            player.transform.position = targetPos;

            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.linearVelocity = Vector2.zero;
        }

        // Update state
        currCheckPointIndex = nextIndex;
        LatestCheckPointLocation = targetPos;
        currentCheckPoint = nextCheckpoint;

        AudioManager.Instance.PlaySFX("Checkpoint");
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
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            player.transform.position = LatestCheckPointLocation;

            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.linearVelocity = Vector2.zero;

            AudioManager.Instance.PlaySFX("Respawn", 0.7f);

        }

        if (currentCheckPoint != null)
        {
            currentCheckPoint.ResetEnemies();
            currentCheckPoint.ResetBoss();
            currentCheckPoint.ResetRace();
        }
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
