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
            currentCheckPoint.ResetEnemies();

        if (currentCheckPoint != null)
            currentCheckPoint.ResetBoss();
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
