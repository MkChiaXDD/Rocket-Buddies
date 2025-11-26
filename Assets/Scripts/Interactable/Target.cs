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
        gameObject.GetComponent<SpriteRenderer>().color = Color.red;
        StartCoroutine(Reset());

        if (door)
        {
            //Open door coroutine
            StartCoroutine(OpenCloseDoor());
        }
    }

    public bool GetIsHit()
    {
        return isHit;
    }

    private IEnumerator Reset()
    {
        yield return new WaitForSeconds(2f);

        isHit = false;
        gameObject.GetComponent<SpriteRenderer>().color = Color.white;
    }

    private IEnumerator OpenCloseDoor()
    {
        door.SetDoor(true);

        yield return new WaitForSeconds(openDoorDuration);

        door.SetDoor(false);
    }
}
