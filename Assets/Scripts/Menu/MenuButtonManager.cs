using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio;

public class MenuButtonManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject settingsPanel;

    [Header("Audio")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider bgmSlider;

    private const string MASTER_KEY = "MasterVolume";
    private const string SFX_KEY = "SFXVolume";
    private const string BGM_KEY = "BGMVolume";

    private bool loading = false;

    private void Start()
    {
        LoadAudio();
        settingsPanel.SetActive(false);
    }

    // ================= BUTTONS =================

    public void PlayButton()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
        LoadAudio();
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
    }

    // ================= AUDIO =================

    private void LoadAudio()
    {
        loading = true;

        masterSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(MASTER_KEY, 0.75f));
        sfxSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(SFX_KEY, 0.75f));
        bgmSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(BGM_KEY, 0.75f));

        ApplyMixer("Master", masterSlider.value);
        ApplyMixer("SFX", sfxSlider.value);
        ApplyMixer("BGM", bgmSlider.value);

        loading = false;
    }

    public void OnMasterChanged(float value)
    {
        if (loading) return;
        ApplyMixer("Master", value);
        PlayerPrefs.SetFloat(MASTER_KEY, value);
        PlayerPrefs.Save();
    }

    public void OnSFXChanged(float value)
    {
        if (loading) return;
        ApplyMixer("SFX", value);
        PlayerPrefs.SetFloat(SFX_KEY, value);
        PlayerPrefs.Save();
    }

    public void OnBGMChanged(float value)
    {
        if (loading) return;
        ApplyMixer("BGM", value);
        PlayerPrefs.SetFloat(BGM_KEY, value);
        PlayerPrefs.Save();
    }

    private void ApplyMixer(string param, float value)
    {
        float v = Mathf.Clamp(value, 0.0001f, 1f);
        audioMixer.SetFloat(param, Mathf.Log10(v) * 20f);
    }
}
