using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BossArenaManager : MonoBehaviour
{
    [SerializeField] private BossController boss;
    [SerializeField] private BossHealthManager bossHp;
    [SerializeField] private BossAnimationController bossAnim;

    [Header("Chainsaw Attack")] 
    [SerializeField] private Transform chainsaw;
    [SerializeField] private Transform chainsawLeftPos, chainsawRightPos;
    [SerializeField] private GameObject warningLeft, warningRight;
    [SerializeField] private float moveSpeed = 3f;

    [Header("Spike Fall Attack")]
    [SerializeField] private GameObject spikeWarning;
    [SerializeField] private List<GameObject> spikes;
    [SerializeField] private Transform spikeStopPoint;
    [SerializeField] private int minSpikes;
    [SerializeField] private int maxSpikes;
    [SerializeField] private float spikeDropCooldown;
    [SerializeField] private float spikeFallSpeed;

    [Header("Boss Pylons")]
    [SerializeField] private List<GameObject> pylons;

    private Coroutine spikeCoroutine;
    private Coroutine chainsawCoroutine;

    public void Reset()
    {
        StopAllCoroutines(); // safest here

        spikeCoroutine = null;
        chainsawCoroutine = null;

        SetSpikeInactive();
        SetPylonInactive();

        chainsaw.position = chainsawLeftPos.position;

        warningLeft.SetActive(false);
        warningRight.SetActive(false);
        spikeWarning.SetActive(false);

        boss.Reset();
        bossHp.Reset();
        bossAnim.PlayIdleAnim();
    }


    private void Start()
    {
        SetSpikeInactive();
        SetPylonInactive();

        warningLeft.SetActive(false);
        warningRight.SetActive(false);
        spikeWarning.SetActive(false);
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
        int LorR = Random.Range(0, 2);

        if (chainsawCoroutine != null)
            StopCoroutine(chainsawCoroutine);

        chainsawCoroutine = StartCoroutine(ChainsawAttack(LorR));
    }

    private IEnumerator ChainsawAttack(int LorR)
    {
        // LorR: 0 = Left, 1 = Right
        bool isLeft = LorR == 0;

        var cam = FindFirstObjectByType<CameraController>();
        cam?.SetSharedWithTarget(chainsaw);

        chainsaw.GetComponent<BossChainsaw>()?.ResetDamagePlayer();
        Debug.Log("BOSS ATTACK: CHAINSAW START");

        Transform startPos = isLeft ? chainsawLeftPos : chainsawRightPos;
        Transform endPos = isLeft ? chainsawRightPos : chainsawLeftPos;
        GameObject warning = isLeft ? warningLeft : warningRight;

        chainsaw.position = startPos.position;
        bossAnim.StartAbilityAnim();
        AudioManager.Instance.PlaySFX("BossSawAlert", 0.1f);
        // Warning flash
        for (int i = 0; i < 3; i++)
        {
            warning.SetActive(true);
            yield return new WaitForSeconds(0.15f);

            warning.SetActive(false);
            yield return new WaitForSeconds(0.15f);
        }

        AudioManager.Instance.PlaySFX("BossSaw");
        // Move chainsaw across arena
        Vector2 targetPos = endPos.position;

        while (Vector2.Distance(chainsaw.position, targetPos) > 0.1f)
        {
            chainsaw.position = Vector2.MoveTowards(
                chainsaw.position,
                targetPos,
                moveSpeed * Time.deltaTime
            );
            yield return null;
        }

        bossAnim.EndAbilityAnim();
        cam?.SetSharedWithTarget(boss.transform);
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
        int amt = Random.Range(minSpikes, maxSpikes);

        if (spikeCoroutine != null)
            StopCoroutine(spikeCoroutine);

        spikeCoroutine = StartCoroutine(SpikeFallAttack(amt));
    }

    private IEnumerator SpikeFallAttack(int amt)
    {
        bossAnim.StartAbilityAnim();
        Debug.Log("BOSS ATTACK: SPIKE FALL START");
        List<GameObject> available = new List<GameObject>(spikes);
        List<GameObject> chosen = new List<GameObject>();

        for (int i = 0; i < amt; i++)
        {
            int index = Random.Range(0, available.Count);
            chosen.Add(available[index]);
            available.RemoveAt(index);
        }

        AudioManager.Instance.PlaySFX("BossSawAlert", 0.1f);
        for (int i = 0; i < 3; i++)
        {
            spikeWarning.SetActive(true);
            yield return new WaitForSeconds(0.15f);

            spikeWarning.SetActive(false);
            yield return new WaitForSeconds(0.15f);
        }

        foreach (GameObject spike in chosen)
        {
            StartCoroutine(DropSingleSpike(spike));
            yield return new WaitForSeconds(spikeDropCooldown);
        }

        bossAnim.EndAbilityAnim();
        yield return new WaitForSeconds(0.5f);
        boss.NextState();
        Debug.Log("BOSS ATTACK: SPIKE FALL END");
    }

    private IEnumerator DropSingleSpike(GameObject spike)
    {
        spike.SetActive(true);
        spike.GetComponent<Spike>()?.ResetHasHit();
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

        AudioManager.Instance.PlaySFX("SpikeLand", 0.3f);

        // Stay down briefly
        yield return new WaitForSeconds(0.25f);

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
