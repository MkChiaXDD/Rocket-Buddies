using UnityEngine;

public class PostProcessTrigger : MonoBehaviour
{
    public enum ZoneType
    {
        Sunny,
        ColdMountain,
        Cave,
        Mansion
    }

    [SerializeField] private ZoneType zoneType;
    [SerializeField] private PostProcessingSwitcher ppSwitcher;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        if (zoneType == ZoneType.Sunny)
            ppSwitcher.SetSunny();
        else if (zoneType == ZoneType.ColdMountain)
            ppSwitcher.SetColdMountain();
        else if (zoneType == ZoneType.Cave)
            ppSwitcher.SetCave();
        else
            ppSwitcher.SetMansion();
    }
}
