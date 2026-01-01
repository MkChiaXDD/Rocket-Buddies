using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class BossController : MonoBehaviour
{
    private States currentState;
    private int currentAttack;
    private bool isAttacking;

    [SerializeField] private Transform idlePos;
    [SerializeField] private BossAnimationController bossAnim;

    [Header("Health Settings")]
    [SerializeField] private int MaxHealth = 30;
    private int currHealth;
    [SerializeField] private SpriteRenderer sprite;

    [Header("Shoot Settings")]
    [SerializeField] private BossBulletPool bossBulletPool;
    [SerializeField] private float bulletSpeed = 1.0f;
    [SerializeField] private int bulletCount = 3;
    [SerializeField] private float timeToNextShot = 1f;

    [Header("Charge Settings")]
    [SerializeField] private float chargeSpeed = 2f;
    [SerializeField] private int numberOfCharges = 3;
    [SerializeField] private float chargeCooldown = 1f;

    [Header("Arena Attacks")]
    [SerializeField] private BossArenaManager arena;
    private enum States
    {
        idle,
        shoot,
        charge,
        chainsaw,
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentAttack = 0;
        isAttacking = false;
        currHealth = MaxHealth;

        if (bossBulletPool == null)
        {
            bossBulletPool = FindFirstObjectByType<BossBulletPool>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentAttack)
        {
            //Idle
            case 0:
                break;
            //Shoot
            case 1:
                if (isAttacking) return;

                isAttacking = true;
                StartCoroutine(PerformShootAttack());
                break;
            //Charge
            case 2:
                if (isAttacking) return;

                isAttacking = true;
                StartCoroutine(PerformChargeAttack());
                break;
            case 3:
                if (isAttacking) return;

                isAttacking = true;
                StartCoroutine(GoIdlePos());
                arena.PerformChainsawAttack();
                break;
            case 4:
                if (isAttacking) return;

                isAttacking = true;
                StartCoroutine(GoIdlePos());
                arena.PerformSpikeFallAttack();
                break;
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            StartBossFight();
        }
    }

    public void StartBossFight()
    {
        currentAttack = 1;
    }


    #region Shooting Attack

    private IEnumerator PerformShootAttack()
    {
        Debug.Log("BOSS ATTACK: SHOOT START");
        for (int i = 0; i < bulletCount; i++)
        {
            bossAnim.PlayShootAnim();
            yield return new WaitForSeconds(0.3f);
            ShootBullet();
            yield return new WaitForSeconds(timeToNextShot);
        }

        Debug.Log("BOSS ATTACK: SHOOT END");
        NextState();
    }

    private void ShootBullet()
    {
        Transform shootTarget = null;
        while (shootTarget == null)
        {
            shootTarget = ChooseCurrentTarget();
            if (shootTarget != null)
            {
                break;
            }
        }

        bool faceLeft = shootTarget.position.x < transform.position.x;
        bossAnim.SetFacing(faceLeft);
        Vector2 shootDir = (shootTarget.position - transform.position).normalized;

        GameObject newBullet = bossBulletPool.GetObject();
        newBullet.transform.position = transform.position;
        BossBullet newBulletScript = newBullet.GetComponent<BossBullet>();
        newBulletScript.Init(shootDir, bulletSpeed);
    }

    #endregion

    #region Charge Attack

    private IEnumerator PerformChargeAttack()
    {
        Debug.Log("BOSS ATTACK: CHARGE ATTACK START");
        for (int i = 0; i < numberOfCharges; i++)
        {
            Transform target = null;
            while (target == null)
            {
                target = ChooseCurrentTarget();
                if (target != null)
                {
                    break;
                }
            }
            Vector2 chargeTargetPos = target.position;
            bool faceLeft = target.position.x < transform.position.x;
            bossAnim.SetFacing(faceLeft);

            while (Vector2.Distance(transform.position, chargeTargetPos) > 0.1f)
            {
                transform.position = Vector2.MoveTowards(transform.position, chargeTargetPos, chargeSpeed * Time.deltaTime);
                yield return null;
            }

            yield return new WaitForSeconds(chargeCooldown);
        }

        Debug.Log("BOSS ATTACK: CHARGE ATTACK END");
        NextState();
    }

    #endregion

    #region Helper Functions

    public Transform ChooseCurrentTarget()
    {
        Transform player1 = GameObject.Find("Player1")?.transform;
        Transform player2 = GameObject.Find("Player2")?.transform;

        float distPlayer1 = Vector2.Distance(player1.position, transform.position);
        float distPlayer2 = Vector2.Distance(player2.position, transform.position);

        if (distPlayer1 < distPlayer2)
        {
            return player1;
        }
        else
        {
            return player2;
        }
    }

    public void NextState()
    {
        isAttacking = false;
        currentAttack++;
    }

    #endregion

    #region Go Idle

    private IEnumerator GoIdlePos()
    {
        Vector2 targetPos = idlePos.position;

        while (Vector2.Distance(transform.position, targetPos) > 0.1f)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPos, chargeSpeed * Time.deltaTime);
            yield return null;
        }
    }

    #endregion
}

