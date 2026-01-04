using System.Collections;
using UnityEngine;

public class Target : MonoBehaviour
{
    private bool isHit = false;

    [SerializeField] private float openDoorDuration = 3f;
    [SerializeField] private Door door;

    // ?? ADD THIS
    [SerializeField] private Lazer lazer;

    public void OnHit()
    {
        if (isHit) return;

        AudioManager.Instance.PlaySFX("TargetHit");
        isHit = true;
        GetComponent<SpriteRenderer>().color = Color.red;

        StartCoroutine(Reset());

        // Door logic (UNCHANGED)
        if (door != null)
            StartCoroutine(OpenCloseDoor());

        // ?? ADD THIS (Laser logic)
        if (lazer != null)
            lazer.DeactivateLazer(openDoorDuration);
    }

    public bool GetIsHit()
    {
        return isHit;
    }

    private IEnumerator Reset()
    {
        yield return new WaitForSeconds(openDoorDuration);
        isHit = false;
        GetComponent<SpriteRenderer>().color = Color.white;
    }

    private IEnumerator OpenCloseDoor()
    {
        door.OpenDoor();
        door.StartTimer(openDoorDuration);

        yield return new WaitForSeconds(openDoorDuration);

        door.CloseDoor();
    }
}
