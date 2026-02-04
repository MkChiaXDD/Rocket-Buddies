using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BossHealthManager : MonoBehaviour
{
    [SerializeField] private Image hpBg;
    [SerializeField] private Image healthBar;
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private GameObject bossName;
    [SerializeField] private int maxHealth;
    [SerializeField] private GameObject whitePanel;
    [SerializeField] private float panelFadeDuration = 1f;
    [SerializeField] private TMP_Text endText;
    [SerializeField] private float endTextFadeDuration = 0.75f;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private int returnCountdownSeconds = 10;
    [SerializeField] private float delayAfterEndText = 2f;
    private int currHealth;
    private bool isDead = false;

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
        if (isDead) return;
        if (currHealth > 0)
        {
            currHealth--;
            if (currHealth <= 0)
            {
                StartCoroutine(DieSequence());
                FindFirstObjectByType<BossAnimationController>()?.PlayDieAnim();
                isDead = true;
            }
        }
        StartCoroutine(DamageFlash());
        UpdateHealthBar();
    }

    private IEnumerator DieSequence()
    {
        if (whitePanel == null) yield break;

        Image img = whitePanel.GetComponent<Image>();
        if (img == null) yield break;

        gameObject.GetComponent<BossController>().BossDie();
        FindFirstObjectByType<BossArenaManager>().BossDie();
        FindFirstObjectByType<CameraController>().SetSharedWithTarget(gameObject.transform);

        // hide texts at start of sequence
        AudioManager.Instance.PlaySFX("BossDie");
        FindFirstObjectByType<CameraController>()?.StartSharedShake(0.2f);
        if (endText != null) endText.gameObject.SetActive(false);
        if (timerText != null) timerText.gameObject.SetActive(false);

        whitePanel.SetActive(true);

        // ===== 1) Fade panel to alpha 1 =====
        Color panelC = img.color;
        float startA = panelC.a;
        float t = 0f;

        while (t < panelFadeDuration)
        {
            t += Time.deltaTime;
            panelC.a = Mathf.Lerp(startA, 1f, t / panelFadeDuration);
            img.color = panelC;
            yield return null;
        }

        panelC.a = 1f;
        img.color = panelC;
        FindFirstObjectByType<CameraController>()?.StopSharedShake();

        // ===== 2) Fade in endText =====
        if (endText != null)
        {
            endText.gameObject.SetActive(true);

            Color endC = endText.color;
            endC.a = 0f;
            endText.color = endC;

            float tt = 0f;
            while (tt < endTextFadeDuration)
            {
                tt += Time.deltaTime;
                endC.a = Mathf.Lerp(0f, 1f, tt / endTextFadeDuration);
                endText.color = endC;
                yield return null;
            }

            endC.a = 1f;
            endText.color = endC;
        }

        // ===== 3) Wait 2s =====
        yield return new WaitForSeconds(delayAfterEndText);

        // ===== 4) Show timer instantly + countdown =====
        if (timerText != null)
        {
            timerText.gameObject.SetActive(true);

            for (int i = returnCountdownSeconds; i > 0; i--)
            {
                timerText.text = $"Returning to menu in {i}s";
                yield return new WaitForSeconds(1f);
            }

            timerText.text = $"Returning to menu in 0s";
            yield return new WaitForSeconds(1.5f);
        }

        SceneManager.LoadScene("MainMenu");
    }


    public void Heal()
    {
        if (currHealth < maxHealth)
        {
            currHealth++;
            AudioManager.Instance.PlaySFX("BossHeal");
            StartCoroutine(HealFlash());
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

    private IEnumerator HealFlash()
    {
        sprite.color = Color.green;

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
