using UnityEngine;

public class BGMTriggerBlock : MonoBehaviour
{
    [SerializeField] private string bgmName;
    [SerializeField] private bool loop = true;

    private void OnTriggerEnte2D(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        AudioManager.Instance.PlayBGM(bgmName, loop);
    }
}
