using UnityEngine;

public class WorldManager : MonoBehaviour
{
    [SerializeField] private GameObject StartingDoor;
    [SerializeField] private GameObject WorldSpaceCanvas;

    [SerializeField] private string worldOneBGM;

    private void Start()
    {
        if (StartingDoor) StartingDoor.SetActive(true);
        if (WorldSpaceCanvas) WorldSpaceCanvas.SetActive(false);
    }

    public void StartGame()
    {
        if (StartingDoor) StartingDoor.SetActive(false);
        if (WorldSpaceCanvas) WorldSpaceCanvas.SetActive(true);

        PlayWorldBGM(worldOneBGM);
    }

    private void PlayWorldBGM(string name)
    {
        AudioManager.Instance.StopBGM();
        AudioManager.Instance.PlayBGM(name, true, 0.01f);
    }
}
