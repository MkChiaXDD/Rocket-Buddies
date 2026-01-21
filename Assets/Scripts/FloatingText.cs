using System.Collections;
using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    public static FloatingText Instance;

    [SerializeField] private TMP_Text text;

    [Header("Positions")]
    public float startX = 900f;
    public float centerX = 0f;
    public float endX = -900f;

    [Header("Speeds")]
    public float fastSpeed = 2400f;
    public float slowSpeed = 450f;

    [Header("Center Slow Zone")]
    public float slowRange = 180f;      // ± range around center
    public float minSlowTime = 0.6f;    // how long it lingers while moving

    private RectTransform rect;
    private Coroutine routine;

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

        float x = startX;
        rect.anchoredPosition = new Vector2(x, 0);

        float slowTimer = 0f;
        bool exitedCenter = false;

        while (!exitedCenter)
        {
            float distToCenter = Mathf.Abs(x - centerX);
            bool inSlowZone = distToCenter <= slowRange;

            float speed = inSlowZone ? slowSpeed : fastSpeed;

            x += Mathf.Sign(centerX - x) * speed * Time.deltaTime;
            rect.anchoredPosition = new Vector2(x, 0);

            if (inSlowZone)
                slowTimer += Time.deltaTime;

            // Once we've lingered long enough and passed center ? exit
            if (slowTimer >= minSlowTime && x <= centerX)
                exitedCenter = true;

            yield return null;
        }

        // Fast exit to the left
        yield return MoveLinear(x, endX, fastSpeed);

        text.enabled = false;
    }

    // ================= EXIT =================

    private IEnumerator MoveLinear(float from, float to, float speed)
    {
        float x = from;
        float dir = Mathf.Sign(to - from);

        while ((dir > 0 && x < to) || (dir < 0 && x > to))
        {
            x += dir * speed * Time.deltaTime;
            rect.anchoredPosition = new Vector2(x, 0);
            yield return null;
        }

        rect.anchoredPosition = new Vector2(to, 0);
    }
}
