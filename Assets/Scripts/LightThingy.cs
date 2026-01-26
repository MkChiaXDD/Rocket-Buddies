using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightThingy : MonoBehaviour
{
    [SerializeField] private Light2D theLight;

    [Header("Flicker Settings")]
    [SerializeField] private float upDownSpeed = 1f;
    [SerializeField] private float minIntensity = 0.8f;
    [SerializeField] private float maxIntensity = 1.2f;

    private float timer;

    private void Update()
    {
        if (theLight == null) return;

        timer += Time.deltaTime * upDownSpeed;

        float t = Mathf.PingPong(timer, 1f);
        theLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, t);
    }
}
