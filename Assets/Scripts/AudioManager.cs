using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [System.Serializable]
    public class NamedClip
    {
        public string name;      // e.g. "MainTheme", "Jump", "Explosion"
        public AudioClip clip;
    }

    [Header("BGM")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private List<NamedClip> bgmClips = new List<NamedClip>();

    [Header("SFX")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private List<NamedClip> sfxClips = new List<NamedClip>();

    private Dictionary<string, AudioClip> bgmDict;
    private Dictionary<string, AudioClip> sfxDict;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        BuildDictionaries();
    }

    private void BuildDictionaries()
    {
        bgmDict = new Dictionary<string, AudioClip>();
        foreach (var nc in bgmClips)
        {
            if (!string.IsNullOrEmpty(nc.name) && nc.clip != null)
            {
                if (!bgmDict.ContainsKey(nc.name))
                    bgmDict.Add(nc.name, nc.clip);
                else
                    Debug.LogWarning($"Duplicate BGM name: {nc.name}");
            }
        }

        sfxDict = new Dictionary<string, AudioClip>();
        foreach (var nc in sfxClips)
        {
            if (!string.IsNullOrEmpty(nc.name) && nc.clip != null)
            {
                if (!sfxDict.ContainsKey(nc.name))
                    sfxDict.Add(nc.name, nc.clip);
                else
                    Debug.LogWarning($"Duplicate SFX name: {nc.name}");
            }
        }
    }

    // ---------------- BGM ----------------

    public void PlayBGM(string name, bool loop = true, float volume = 1f)
    {
        if (!bgmDict.TryGetValue(name, out var clip))
        {
            Debug.LogWarning($"BGM not found: {name}");
            return;
        }

        if (bgmSource == null)
        {
            Debug.LogWarning("BGM AudioSource is not assigned.");
            return;
        }

        bgmSource.clip = clip;
        bgmSource.loop = loop;
        bgmSource.volume = volume;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        if (bgmSource != null)
            bgmSource.Stop();
    }

    public void PauseBGM()
    {
        if (bgmSource != null)
            bgmSource.Pause();
    }

    public void ResumeBGM()
    {
        if (bgmSource != null)
            bgmSource.UnPause();
    }

    // ---------------- SFX ----------------

    public void PlaySFX(string name, float volume = 1f)
    {
        if (!sfxDict.TryGetValue(name, out var clip))
        {
            Debug.LogWarning($"SFX not found: {name}");
            return;
        }

        if (sfxSource == null)
        {
            Debug.LogWarning("SFX AudioSource is not assigned.");
            return;
        }

        sfxSource.PlayOneShot(clip, volume);
    }

    // Optional: global volume controls
    public void SetBGMVolume(float volume)
    {
        if (bgmSource != null)
            bgmSource.volume = volume;
    }

    public void SetSFXVolume(float volume)
    {
        if (sfxSource != null)
            sfxSource.volume = volume;
    }
}
