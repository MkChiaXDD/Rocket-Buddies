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

    [Header("Shoot Settings")]
    [SerializeField] private BossBulletPool bossBulletPool;
    [SerializeField] private float bulletSpeed = 1.0f;
    [SerializeField] private int bulletCount = 3;
    [SerializeField] private float timeToNextShot = 1f;
    [SerializeField] private float shootRecoilDistance = 0.15f;

    [Header("Charge Settings")]
    [SerializeField] private ParticleSystem dashAfterImageFX;
    [SerializeField] private float chargeSpeed = 2f;
    [SerializeField] private int numberOfCharges = 3;
    [SerializeField] private float chargeCooldown = 1f;
    private bool chargeCanHit = false;

    [Header("Direct Hit Settings")]
    [SerializeField] private int attackCount = 3;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float timeTillImpact = 3f;
    [SerializeField] private float indicatorSize = 3f;
    [SerializeField] private BossDirectHitIndicatorPool indicatorPool;

    [Header("Arena Attacks")]
    [SerializeField] private BossArenaManager arena;

    [Header("Health Things")]
    [SerializeField] private BossHealthManager hpMgr;
    private enum States
    {
        idle,
        shoot,
        charge,
        chainsaw,
    }

    public void Reset()
    {
        StopCoroutine(PerformChargeAttack());
        StopCoroutine(PerformShootAttack());
        transform.position = idlePos.position;
        currentAttack = 0;
        isAttacking = false;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentAttack = 0;
        isAttacking = false;

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
                StartCoroutine(GoIdlePos());
                arena.PerformChainsawAttack();
                break;
            case 3:
                if (isAttacking) return;

                isAttacking = true;
                StartCoroutine(PerformChargeAttack());
                break;
            case 4:
                if (isAttacking) return;

                isAttacking = true;
                StartCoroutine(GoIdlePos());
                arena.PerformSpikeFallAttack();
                break;
            //case 5:
            //    if (isAttacking) return;

            //    isAttacking = true;
            //    StartCoroutine(GoIdlePos());
            //    StartCoroutine(PerformDirectHitAttack());
            //    break;
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            StartBossFight();
        }
    }

    public void StartBossFight()
    {
        hpMgr.ShowHpBar();
        StartCoroutine(DelayStartBossFight());   
    }

    private IEnumerator DelayStartBossFight()
    {
        yield return new WaitForSeconds(2f);

        currentAttack = 1;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && chargeCanHit)
        {
            FindFirstObjectByType<HealthManager>()?.Damage(1);
            chargeCanHit = false;
        }
    }

    #region Shooting Attack

    private IEnumerator PerformShootAttack()
    {
        Debug.Log("BOSS ATTACK: SHOOT START");
        for (int i = 0; i < bulletCount; i++)
        {
            bossAnim.PlayShootAnim();
            yield return new WaitForSeconds(0.3f);
            AudioManager.Instance.PlaySFX("BossShoot");
            ShootBullet();
            yield return new WaitForSeconds(timeToNextShot);
        }

        yield return new WaitForSeconds(0.5f);
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

        transform.position -= (Vector3)(shootDir * shootRecoilDistance);
    }

    #endregion

    #region Charge Attack

    private IEnumerator PerformChargeAttack()
    {
        Debug.Log("BOSS ATTACK: CHARGE ATTACK START");
        dashAfterImageFX.Play();
        yield return new WaitForSeconds(chargeCooldown);
        for (int i = 0; i < numberOfCharges; i++)
        {
            chargeCanHit = true;
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

            // Flip dash afterimage FX to match boss
            Vector3 fxScale = dashAfterImageFX.transform.localScale;
            fxScale.x = Mathf.Abs(fxScale.x) * (faceLeft ? -1f : 1f);
            dashAfterImageFX.transform.localScale = fxScale;
            AudioManager.Instance.PlaySFX("BossCharge");
            while (Vector2.Distance(transform.position, chargeTargetPos) > 0.1f)
            {
                transform.position = Vector2.MoveTowards(transform.position, chargeTargetPos, chargeSpeed * Time.deltaTime);
                yield return null;
            }

            yield return new WaitForSeconds(chargeCooldown);
        }

        Debug.Log("BOSS ATTACK: CHARGE ATTACK END");
        dashAfterImageFX.Stop();
        NextState();
    }

    #endregion

    #region Direct Hit Attack
    private IEnumerator PerformDirectHitAttack()
    {
        for (int i = 0; i < attackCount; i++)
        {
            Transform target = ChooseCurrentTarget();
            if (target == null) yield break;

            GameObject indicatorObj = indicatorPool.GetObject();
            BossDirectHitIndicator indicator =
                indicatorObj.GetComponent<BossDirectHitIndicator>();

            indicator.Init(target.position, timeTillImpact, indicatorSize, indicatorPool);

            yield return new WaitForSeconds(attackCooldown);
        }

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
        if (currentAttack > 4)
        {
            arena.SpawnPylons();
            currentAttack = 1;
        }
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

