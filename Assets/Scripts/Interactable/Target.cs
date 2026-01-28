using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    private bool isHit = false;

    [SerializeField] private float openDoorDuration = 3f;
    [SerializeField] private Door door;

    // ?? ADD THIS
    [SerializeField] private Lazer lazer;

    [SerializeField] private float appearDuration = 2f;
    [SerializeField] private List<GameObject> objectAppear;

    [Header("Random Door Settings")]
    [SerializeField] private List<Door> randomDoors;
    [SerializeField] private bool closeRandomDoor = true;

    [Header("Laser Race Thing")]
    [SerializeField] private LaserRace race;


    private void Start()
    {
        for (int i = 0; i < objectAppear.Count; i++)
        {
            objectAppear[i].SetActive(false);
        }
    }

    public void OnHit()
    {
        if (isHit) return;

        AudioManager.Instance.PlaySFX("TargetHit");
        isHit = true;
        GetComponent<SpriteRenderer>().color = Color.red;

        StartCoroutine(Reset());

        // Door logic (UNCHANGED)
        if (door != null && openDoorDuration > 0)
        {
            if (openDoorDuration > 0)
            {
                StartCoroutine(OpenCloseDoor());
            }
        }
        else if (door != null && openDoorDuration <= 0)
        {
            door.OpenDoor();
        }

        // ?? ADD THIS (Laser logic)
        if (lazer != null)
            lazer.DeactivateLazer(openDoorDuration);

        if (objectAppear != null)
            StartCoroutine(OnOffObject());

        // Random door logic
        if (randomDoors != null && randomDoors.Count > 0)
        {
            OpenRandomDoor();
        }

        if (race != null)
        {
            StartLaserRace();
        }
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

    private IEnumerator OnOffObject()
    {
        for (int i = 0; i < objectAppear.Count; i++)
        {
            objectAppear[i].SetActive(true);
        }

        yield return new WaitForSeconds(appearDuration);

        for (int i = 0; i < objectAppear.Count; i++)
        {
            objectAppear[i].SetActive(false);
        }
    }

    private void OpenRandomDoor()
    {
        int randomIndex = Random.Range(0, randomDoors.Count);
        Door selectedDoor = randomDoors[randomIndex];

        if (selectedDoor == null) return;

        selectedDoor.OpenDoor();

        if (closeRandomDoor && openDoorDuration > 0)
        {
            StartCoroutine(CloseRandomDoorAfterTime(selectedDoor));
        }
    }

    private IEnumerator CloseRandomDoorAfterTime(Door doorToClose)
    {
        yield return new WaitForSeconds(openDoorDuration);
        doorToClose.CloseDoor();
    }

    private void StartLaserRace()
    {
        race.StartRace();
    }
}
