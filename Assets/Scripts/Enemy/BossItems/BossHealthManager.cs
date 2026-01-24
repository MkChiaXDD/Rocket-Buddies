using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BossHealthManager : MonoBehaviour
{
    [SerializeField] private Image hpBg;
    [SerializeField] private Image healthBar;
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private GameObject bossName;
    [SerializeField] private int maxHealth;
    private int currHealth;

    public void Reset()
    {
        currHealth = maxHealth;
        UpdateHealthBar();
        hpBg.enabled = false;
        healthBar.enabled = false;
        bossName.SetActive(false);
    }

    public void ShowHpBar()
    {
        hpBg.enabled = true;
        healthBar.enabled = true;
        bossName.SetActive(true);
    }

    private void Start()
    {
        currHealth = maxHealth;

        hpBg.enabled = false;
        healthBar.enabled = false;
        bossName.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            Damage();
        }
    }

    public void Damage()
    {
        if (currHealth > 0)
        {
            currHealth--;
        }
        else
        {
            Destroy(gameObject);
        }
        StartCoroutine(DamageFlash());
        UpdateHealthBar();
    }

    public void Heal()
    {
        if (currHealth < maxHealth)
        {
            currHealth++;
            UpdateHealthBar();
        }
    }

    private IEnumerator DamageFlash()
    {
        Debug.Log("Boss Damaged");
        sprite.color = Color.red;

        yield return new WaitForSeconds(0.1f);

        sprite.color = Color.white;
    }

    protected virtual void UpdateHealthBar()
    {
        if (healthBar == null) return;

        float normalizedHp = (float)currHealth / maxHealth;
        healthBar.fillAmount = normalizedHp;
    }
}
