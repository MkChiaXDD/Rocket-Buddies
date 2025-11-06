using UnityEngine;

public class WorldOneManager : MonoBehaviour
{
    [SerializeField] private GameObject StartingDoor;
    [SerializeField] private GameObject WorldSpaceCanvas;

    private void Start()
    {
        if (StartingDoor) StartingDoor.SetActive(true);
        if (WorldSpaceCanvas) WorldSpaceCanvas.SetActive(false);
    }

    public void StartGame()
    {
        if (StartingDoor) StartingDoor.SetActive(false);
        if (WorldSpaceCanvas) WorldSpaceCanvas.SetActive(true);
    }
}
