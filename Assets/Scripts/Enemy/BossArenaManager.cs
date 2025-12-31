using System.Collections;
using UnityEngine;

public class BossArenaManager : MonoBehaviour
{
    [SerializeField] private BossController boss;

    [Header("Chainsaw Attack")]
    [SerializeField] private Transform chainsaw;
    [SerializeField] private Transform chainsawLeftPos, chainsawRightPos;
    [SerializeField] private float moveSpeed = 3f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
            
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PerformChainsawAttack()
    {
        //0 = Left, 1 = Right
        int LorR = Random.Range(0, 2);

        StartCoroutine(ChainsawAttack(LorR));
    }

    private IEnumerator ChainsawAttack(int LorR)
    {
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

        boss.NextState();
    }
}
