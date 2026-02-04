using UnityEngine;

public class BGMTriggerBlock : MonoBehaviour
{
    [SerializeField] private string bgmName;
    [SerializeField] private bool loop = true;
    [SerializeField] [Range(0, 1)] private float volume;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        AudioManager.Instance.PlayBGM(bgmName, loop, volume);
    }
}
