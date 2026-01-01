using System.Collections;
using UnityEngine;

public class BossHealingPylon : MonoBehaviour
{
    [SerializeField] private float fireRate = 2f;
    private float timer;
    [SerializeField] private int maxHp = 2;
    private int currHp;
    private SpriteRenderer sprite;

    private BossHealOrbPool pool;

    private void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        currHp = maxHp;
        pool = FindFirstObjectByType<BossHealOrbPool>();
    }

    public void Reset()
    {
        currHp = maxHp;
        timer = 0;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer > fireRate)
        {
            timer = 0;
            ShootHealingOrb();
        }
    }

    private void ShootHealingOrb()
    {
        //Code this out
        GameObject newOrb = pool.GetObject();
        newOrb.transform.position = transform.position;
    }

    public void Damage()
    {
        currHp--;
        StartCoroutine(DamageFlash());
        if (currHp <= 0)
        {
            sprite.color = Color.white;
            gameObject.SetActive(false);
        }
    }

    private IEnumerator DamageFlash()
    {
        Debug.Log("Boss Damaged");
        sprite.color = Color.red;

        yield return new WaitForSeconds(0.1f);

        sprite.color = Color.white;
    }
}
