using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossArenaManager : MonoBehaviour
{
    [SerializeField] private BossController boss;

    [Header("Chainsaw Attack")]
    [SerializeField] private Transform chainsaw;
    [SerializeField] private Transform chainsawLeftPos, chainsawRightPos;
    [SerializeField] private float moveSpeed = 3f;

    [Header("Spike Fall Attack")]
    [SerializeField] private List<GameObject> spikes;
    [SerializeField] private Transform spikeStopPoint;
    [SerializeField] private int minSpikes;
    [SerializeField] private float spikeDropCooldown;
    [SerializeField] private float spikeFallSpeed;

    [Header("Boss Pylons")]
    [SerializeField] private List<GameObject> pylons;

    private void Start()
    {
        SetSpikeInactive();
        SetPylonInactive();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            SpawnPylons();
        }
    }

    #region Chainsaw Attack
    public void PerformChainsawAttack()
    {
        //0 = Left, 1 = Right
        int LorR = Random.Range(0, 2);

        StartCoroutine(ChainsawAttack(LorR));
    }

    private IEnumerator ChainsawAttack(int LorR)
    {
        FindFirstObjectByType<CameraController>()?.SetSharedWithTarget(chainsaw);
        Debug.Log("BOSS ATTACK: CHAINSAW START");
        Vector2 targetPos;
        if (LorR == 0)
        {
            chainsaw.position = chainsawLeftPos.position;
            targetPos = chainsawRightPos.position;
        }
        else
        {
            chainsaw.position = chainsawRightPos.position;
            targetPos = chainsawLeftPos.position;
        }

        while (Vector2.Distance(chainsaw.position, targetPos) > 0.1f)
        {
            chainsaw.position = Vector2.MoveTowards(chainsaw.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        FindFirstObjectByType<CameraController>()?.SetSharedWithTarget(boss.gameObject.transform);
        boss.NextState();
        Debug.Log("BOSS ATTACK: CHAINSAW END");
    }

    #endregion

    #region Spike Fall Attack

    private void SetSpikeInactive()
    {
        foreach (GameObject spike in spikes)
        {
            spike.SetActive(false);
        }
    }

    public void PerformSpikeFallAttack()
    {
        int amt = Random.Range(minSpikes, spikes.Count);
        StartCoroutine(SpikeFallAttack(amt));
    }

    private IEnumerator SpikeFallAttack(int amt)
    {
        Debug.Log("BOSS ATTACK: SPIKE FALL START");
        List<GameObject> available = new List<GameObject>(spikes);
        List<GameObject> chosen = new List<GameObject>();

        for (int i = 0; i < amt; i++)
        {
            int index = Random.Range(0, available.Count);
            chosen.Add(available[index]);
            available.RemoveAt(index);
        }

        foreach (GameObject spike in chosen)
        {
            StartCoroutine(DropSingleSpike(spike));
            yield return new WaitForSeconds(spikeDropCooldown);
        }

        boss.NextState();
        Debug.Log("BOSS ATTACK: SPIKE FALL END");
    }

    private IEnumerator DropSingleSpike(GameObject spike)
    {
        spike.SetActive(true);

        Vector3 startPos = spike.transform.position;
        Vector3 targetPos = new Vector3(
            startPos.x,
            spikeStopPoint.position.y,
            startPos.z
        );

        // Smooth drop
        while (Vector2.Distance(spike.transform.position, targetPos) > 0.05f)
        {
            spike.transform.position = Vector2.MoveTowards(
                spike.transform.position,
                targetPos,
                spikeFallSpeed * Time.deltaTime
            );
            yield return null;
        }

        // Stay down briefly
        yield return new WaitForSeconds(0.5f);

        spike.transform.position = startPos;
        spike.GetComponent<Spike>().hasHit = false;
        spike.SetActive(false);
    }

    #endregion

    #region PylonThings

    private void SetPylonInactive()
    {
        for (int i = 0; i < pylons.Count; i++)
        {
            pylons[i].SetActive(false);
        }
    }

    public void SpawnPylons()
    {
        for (int i = 0; i < pylons.Count; i++)
        {
            pylons[i].SetActive(true);
            pylons[i].GetComponent<BossHealingPylon>()?.Reset();
        }
    }

    #endregion
}
