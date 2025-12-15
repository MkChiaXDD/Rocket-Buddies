using System.Collections;
using UnityEngine;

public class Target : MonoBehaviour
{
    private bool isHit = false;

    [SerializeField] private float openDoorDuration = 3f;
    [SerializeField] private Door door;

    public void OnHit()
    {
        if (isHit) return;

        isHit = true;
        GetComponent<SpriteRenderer>().color = Color.red;

        StartCoroutine(Reset());

        if (door != null)
            StartCoroutine(OpenCloseDoor());
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
