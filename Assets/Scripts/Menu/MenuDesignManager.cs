using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MenuDesignManager : MonoBehaviour
{
    [Header("Image to Animate")]
    [SerializeField] private Image image;                 // drag your UI Image here
    [SerializeField] private RectTransform imageRect;     // or drag the RectTransform

    [Header("Swipe In Settings")]
    [SerializeField] private Vector2 targetAnchoredPos = Vector2.zero; // final position on screen
    [SerializeField] private float swipeDistance = 600f;               // how far below to start
    [SerializeField] private float swipeDuration = 0.8f;
    [SerializeField]
    private AnimationCurve swipeCurve =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Heartbeat Settings")]
    [SerializeField] private float beatScaleAmount = 0.08f;  // how strong the pulse
    [SerializeField] private float beatSpeed = 2f;           // beats per second-ish

    private Vector2 startPos;
    private Vector3 baseScale;
    private bool heartbeatActive = false;

    private void Awake()
    {
        // Make sure we have a RectTransform to animate
        if (imageRect == null)
        {
            if (image != null)
                imageRect = image.rectTransform;
            else
                imageRect = GetComponent<RectTransform>();
        }
    }

    private void Start()
    {
        if (imageRect == null) return;

        baseScale = imageRect.localScale;

        // start off-screen below
        startPos = targetAnchoredPos + new Vector2(0f, -swipeDistance);
        imageRect.anchoredPosition = startPos;

        StartCoroutine(SwipeInThenHeartbeat());
    }

    private IEnumerator SwipeInThenHeartbeat()
    {
        float t = 0f;

        while (t < swipeDuration)
        {
            t += Time.deltaTime;
            float normalized = Mathf.Clamp01(t / swipeDuration);
            float eased = swipeCurve.Evaluate(normalized);

            imageRect.anchoredPosition = Vector2.Lerp(startPos, targetAnchoredPos, eased);
            yield return null;
        }

        imageRect.anchoredPosition = targetAnchoredPos;
        heartbeatActive = true;
    }

    private void Update()
    {
        if (!heartbeatActive || imageRect == null) return;

        // heartbeat scale using a sine wave
        float offset = Mathf.Sin(Time.time * beatSpeed) * beatScaleAmount;
        imageRect.localScale = baseScale * (1f + offset);
    }

    private void OnDisable()
    {
        if (imageRect != null)
            imageRect.localScale = baseScale;
    }
}
