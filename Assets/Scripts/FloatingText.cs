using System.Collections;
using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    public static FloatingText Instance;

    [SerializeField] private TMP_Text text;

    [Header("Positions (X)")]
    [SerializeField] private float startX = 900f;
    [SerializeField] private float centerX = 0f;
    [SerializeField] private float endX = -900f;

    [Header("Timings (seconds)")]
    [SerializeField] private float enterDuration = 0.6f;
    [SerializeField] private float lingerDuration = 0.8f;
    [SerializeField] private float exitDuration = 0.4f;

    private RectTransform rect;
    private Coroutine routine;

    // ================= UNITY =================

    private void Awake()
    {
        Instance = this;
        rect = GetComponent<RectTransform>();

        text.enabled = false;
        text.raycastTarget = false;
    }

    // ================= PUBLIC =================

    public void Show(string msg)
    {
        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(Animate(msg));
    }

    // ================= ANIMATION =================

    private IEnumerator Animate(string msg)
    {
        text.text = msg;
        text.enabled = true;

        // Start off-screen (right)
        rect.anchoredPosition = new Vector2(startX, 0);

        AudioManager.Instance.PlaySFX("FloatingTextSwoosh");
        // 1?? Enter: right ? center
        yield return MoveTimed(startX, centerX, enterDuration);

        // 2?? Linger at center
        yield return new WaitForSeconds(lingerDuration);

        AudioManager.Instance.PlaySFX("FloatingTextSwoosh");
        // 3?? Exit: center ? left
        yield return MoveTimed(centerX, endX, exitDuration);

        text.enabled = false;
    }

    // ================= HELPERS =================

    private IEnumerator MoveTimed(float from, float to, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Smooth easing (feels much better for UI)
            float eased = Mathf.SmoothStep(0f, 1f, t);
            float x = Mathf.Lerp(from, to, eased);

            rect.anchoredPosition = new Vector2(x, 0);
            yield return null;
        }

        rect.anchoredPosition = new Vector2(to, 0);
    }
}
