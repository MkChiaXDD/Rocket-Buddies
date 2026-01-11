using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class GamePauseMenu : MonoBehaviour
{
    [Header("Pause UI")]
    [SerializeField] private GameObject pausePanel;

    [Header("Audio")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider bgmSlider;

    private bool isLoadingSettings = false;
    private bool callbacksEnabled = false;

    private bool isPaused = false;

    private const string MASTER_KEY = "MasterVolume";
    private const string SFX_KEY = "SFXVolume";
    private const string BGM_KEY = "BGMVolume";

    private void Start()
    {
        pausePanel.SetActive(false);

        masterSlider.onValueChanged.AddListener(OnMasterChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXChanged);
        bgmSlider.onValueChanged.AddListener(OnBGMChanged);
    }

    public void TogglePausePanel()
    {
        isPaused = !isPaused;

        pausePanel.SetActive(isPaused);

        if (isPaused)
        {
            Time.timeScale = 0;
            LoadAudioSettings(); // important
        }
        else
        {
            Time.timeScale = 1;
        }
    }


    private void LoadAudioSettings()
    {
        callbacksEnabled = false;

        float master = PlayerPrefs.GetFloat(MASTER_KEY, 0.75f);
        float sfx = PlayerPrefs.GetFloat(SFX_KEY, 0.75f);
        float bgm = PlayerPrefs.GetFloat(BGM_KEY, 0.75f);

        masterSlider.SetValueWithoutNotify(master);
        sfxSlider.SetValueWithoutNotify(sfx);
        bgmSlider.SetValueWithoutNotify(bgm);

        ApplyMixer("Master", master);
        ApplyMixer("SFX", sfx);
        ApplyMixer("BGM", bgm);

        callbacksEnabled = true;
    }



    private void ApplyMixer(string param, float value)
    {
        float v = Mathf.Clamp(value, 0.0001f, 1f);
        audioMixer.SetFloat(param, Mathf.Log10(v) * 20f);
    }

    public void OnMasterChanged(float value)
    {
        if (!callbacksEnabled) return;

        ApplyMixer("Master", value);
        PlayerPrefs.SetFloat(MASTER_KEY, value);
        PlayerPrefs.Save();
    }

    public void OnSFXChanged(float value)
    {
        if (!callbacksEnabled) return;

        ApplyMixer("SFX", value);
        PlayerPrefs.SetFloat(SFX_KEY, value);
        PlayerPrefs.Save();
    }

    public void OnBGMChanged(float value)
    {
        if (!callbacksEnabled) return;

        ApplyMixer("BGM", value);
        PlayerPrefs.SetFloat(BGM_KEY, value);
        PlayerPrefs.Save();
    }


}
