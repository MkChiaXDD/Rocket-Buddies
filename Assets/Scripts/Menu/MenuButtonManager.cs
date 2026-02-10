using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuButtonManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject instructionsPanel;

    [Header("Audio")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider bgmSlider;

    private const string MASTER_KEY = "MasterVolume";
    private const string SFX_KEY = "SFXVolume";
    private const string BGM_KEY = "BGMVolume";

    private bool loading;

    [Header("Controller Input")]
    [SerializeField] private PlayerInput playerInput;

    [Header("Main Menu Buttons")]
    [SerializeField] private List<Button> menuButtons;
    private int currMenuBtnSel;

    [Header("Settings Sliders")]
    [SerializeField] private List<Slider> settingsSliders;
    private int currSliderSel;

    [Header("Highlight Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.yellow;

    private enum UIMode
    {
        MainMenu,
        Settings
    }

    private UIMode currentMode = UIMode.MainMenu;

    private void Start()
    {
        masterSlider.onValueChanged.AddListener(OnMasterChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXChanged);
        bgmSlider.onValueChanged.AddListener(OnBGMChanged);

        LoadAudio();

        settingsPanel.SetActive(false);
        instructionsPanel.SetActive(false);

        currMenuBtnSel = 0;
        SelectButton(currMenuBtnSel);
    }

    private void Update()
    {
        if (currentMode == UIMode.MainMenu)
            HandleMenuInput();
        else if (currentMode == UIMode.Settings)
            HandleSliderInput();

        if (playerInput.actions["Back"].WasPressedThisFrame())
        {
            if (currentMode == UIMode.Settings)
                CloseSettings();
            else if (instructionsPanel.activeInHierarchy)
                CloseInstructions();
        }
    }

    // ================= BUTTONS =================

    public void PlayButton()
    {
        AudioManager.Instance.PlaySFX("ButtonClick");
        SceneManager.LoadScene("LoadingScene");
    }

    public void OpenSettings()
    {
        AudioManager.Instance.PlaySFX("ButtonClick");
        settingsPanel.SetActive(true);
        LoadAudio();

        currentMode = UIMode.Settings;
        currSliderSel = 0;
        SelectSlider(currSliderSel);
    }

    public void CloseSettings()
    {
        AudioManager.Instance.PlaySFX("ButtonClick");
        settingsPanel.SetActive(false);

        currentMode = UIMode.MainMenu;
        SelectButton(currMenuBtnSel);
    }

    public void OpenInstructions()
    {
        AudioManager.Instance.PlaySFX("ButtonClick");
        instructionsPanel.SetActive(true);
    }

    public void CloseInstructions()
    {
        AudioManager.Instance.PlaySFX("ButtonClick");
        instructionsPanel.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
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
    }

    public void OnSFXChanged(float value)
    {
        if (loading) return;
        ApplyMixer("SFX", value);
        PlayerPrefs.SetFloat(SFX_KEY, value);
    }

    public void OnBGMChanged(float value)
    {
        if (loading) return;
        ApplyMixer("BGM", value);
        PlayerPrefs.SetFloat(BGM_KEY, value);
    }

    private void ApplyMixer(string param, float value)
    {
        float v = Mathf.Clamp(value, 0.0001f, 1f);
        audioMixer.SetFloat(param, Mathf.Log10(v) * 20f);
    }

    // ================= INPUT HANDLING =================

    private void HandleMenuInput()
    {
        if (playerInput.actions["Next"].WasPressedThisFrame())
            currMenuBtnSel--;
        else if (playerInput.actions["Previous"].WasPressedThisFrame())
            currMenuBtnSel++;

        WrapIndex(ref currMenuBtnSel, menuButtons.Count);
        SelectButton(currMenuBtnSel);

        if (playerInput.actions["Submit"].WasPressedThisFrame())
            menuButtons[currMenuBtnSel].onClick.Invoke();
    }

    private void HandleSliderInput()
    {
        if (playerInput.actions["Next"].WasPressedThisFrame())
            currSliderSel--;
        else if (playerInput.actions["Previous"].WasPressedThisFrame())
            currSliderSel++;

        WrapIndex(ref currSliderSel, settingsSliders.Count);
        SelectSlider(currSliderSel);

        float moveX = playerInput.actions["Navigate"].ReadValue<Vector2>().x;

        if (Mathf.Abs(moveX) > 0.5f)
        {
            settingsSliders[currSliderSel].value += moveX * 0.01f;
        }
    }

    // ================= SELECTION =================

    private void SelectButton(int index)
    {
        for (int i = 0; i < menuButtons.Count; i++)
        {
            Image img = menuButtons[i].GetComponent<Image>();
            img.color = (i == index) ? selectedColor : normalColor;

            if (i == index)
                menuButtons[i].Select();
        }
    }

    private void SelectSlider(int index)
    {
        for (int i = 0; i < settingsSliders.Count; i++)
        {
            Image img = settingsSliders[i].GetComponentInChildren<Image>();
            img.color = (i == index) ? selectedColor : normalColor;

            if (i == index)
                settingsSliders[i].Select();
        }
    }

    private void WrapIndex(ref int index, int count)
    {
        if (index < 0) index = count - 1;
        else if (index >= count) index = 0;
    }
}
