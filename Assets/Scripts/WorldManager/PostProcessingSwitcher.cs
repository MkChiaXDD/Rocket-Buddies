using UnityEngine;
using UnityEngine.Rendering;

public class PostProcessingSwitcher : MonoBehaviour
{
    [Header("Volume")]
    [SerializeField] private Volume globalVolume;

    [Header("Profiles")]
    [SerializeField] private VolumeProfile sunnyProfile;
    [SerializeField] private VolumeProfile coldProfile;
    [SerializeField] private VolumeProfile caveProfile;
    [SerializeField] private VolumeProfile mansionProfile;

    public enum ZoneType
    {
        Sunny,
        ColdMountain,
        Cave,
        Mansion
    }

    [SerializeField] private ZoneType baseZoneType;

    private void Start()
    {
        if (baseZoneType == ZoneType.Sunny)
        {
            SetSunny();
        }
        else if (baseZoneType == ZoneType.ColdMountain)
        {
            SetColdMountain();
        }
        else if (baseZoneType == ZoneType.Cave)
        {
            SetCave();
        }
        else
        {
            SetMansion();
        }
    }

    public void SetSunny()
    {
        if (globalVolume != null && sunnyProfile != null)
        {
            Debug.Log("Setting Sunny");
            globalVolume.profile = sunnyProfile;
        }
    }

    public void SetColdMountain()
    {
        if (globalVolume != null && coldProfile != null)
        {
            Debug.Log("Setting Cold");
            globalVolume.profile = coldProfile;
        }
    }

    public void SetCave()
    {
        if (globalVolume != null && caveProfile != null)
        {
            Debug.Log("Setting Save");
            globalVolume.profile = caveProfile;
        }
    }

    public void SetMansion()
    {
        if (globalVolume != null && mansionProfile != null)
        {
            Debug.Log("Setting Mansion");
            globalVolume.profile = mansionProfile;
        }
    }
}
