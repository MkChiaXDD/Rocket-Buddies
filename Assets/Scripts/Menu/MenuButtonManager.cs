using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

public class MenuButtonManager : MonoBehaviour
{
    [SerializeField] private PlayerInput action;

    [Header("Buttons (top ? bottom)")]
    [SerializeField] private List<Button> buttons;

    [Header("Options")]
    [SerializeField] private bool wrapAround = true;
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private Color normalColor = Color.white;

    [Header("Settings Panel")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject IdkPanel;

    [Header("Settings UI")]
    [SerializeField] private TMP_Text masterText;
    [SerializeField] private Slider masterSlider;
    [SerializeField] private TMP_Text sfxText;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private TMP_Text bgmText;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Button settingsCloseButton;

    [Header("Audio")]
    [SerializeField] private AudioMixer audioMixer;

    private int mainIndex = 0;

    // 0 = Master, 1 = SFX, 2 = BGM, 3 = Close
    private int settingsIndex = 0;
    private bool inSettings = false;

    // Input actions
    private InputAction nextAction;
    private InputAction prevAction;
    private InputAction submitAction;
    private InputAction backAction;
    private InputAction louderAction;
    private InputAction softerAction;

    void OnEnable()
    {
        if (action != null && action.actions != null)
        {
            nextAction = action.actions.FindAction("Next");
            prevAction = action.actions.FindAction("Previous");
            submitAction = action.actions.FindAction("Submit");
            backAction = action.actions.FindAction("Back");
            louderAction = action.actions.FindAction("Louder");
            softerAction = action.actions.FindAction("Softer");

            if (nextAction != null) { nextAction.performed += OnNext; nextAction.Enable(); }
            if (prevAction != null) { prevAction.performed += OnPrev; prevAction.Enable(); }
            if (submitAction != null) { submitAction.performed += OnSubmit; submitAction.Enable(); }
            if (backAction != null) { backAction.performed += OnBack; backAction.Enable(); }
            if (louderAction != null) { louderAction.performed += OnLouder; louderAction.Enable(); }
            if (softerAction != null) { softerAction.performed += OnSofter; softerAction.Enable(); }
        }

        if (buttons != null && buttons.Count > 0)
            HighlightMainButton(mainIndex);
    }

    void OnDisable()
    {
        if (nextAction != null) nextAction.performed -= OnNext;
        if (prevAction != null) prevAction.performed -= OnPrev;
        if (submitAction != null) submitAction.performed -= OnSubmit;
        if (backAction != null) backAction.performed -= OnBack;
        if (louderAction != null) louderAction.performed -= OnLouder;
        if (softerAction != null) softerAction.performed -= OnSofter;
    }

    void Start()
    {
        if (buttons != null && buttons.Count > 0)
            HighlightMainButton(0);

        if (masterSlider != null)
            masterSlider.value = 0.75f;

        if (sfxSlider != null)
            sfxSlider.value = 0.75f;

        if (bgmSlider != null)
            bgmSlider.value = 0.75f;

        UpdateAudioMixerFromSliders();

        settingsPanel.SetActive(false);
    }

    // -------------------------------------------------------
    // INPUT WRAPPERS
    // -------------------------------------------------------

    private void OnNext(InputAction.CallbackContext ctx) => Move(+1);
    private void OnPrev(InputAction.CallbackContext ctx) => Move(-1);
    private void OnSubmit(InputAction.CallbackContext ctx)
    {
        if (inSettings) PressSettings();
        else PressMain();
    }

    // Back: closes settings if open
    private void OnBack(InputAction.CallbackContext ctx)
    {
        if (inSettings && settingsPanel != null && settingsPanel.activeSelf)
        {
            CloseSettingsPage();
        }
    }

    private void OnLouder(InputAction.CallbackContext ctx)
    {
        if (!inSettings) return;
        AdjustCurrentSetting(+0.05f);
    }

    private void OnSofter(InputAction.CallbackContext ctx)
    {
        if (!inSettings) return;
        AdjustCurrentSetting(-0.05f);
    }

    // -------------------------------------------------------
    // NAVIGATION
    // -------------------------------------------------------

    private void Move(int dir)
    {
        // If settings is open, move between settings controls instead
        if (inSettings)
        {
            int maxIndex = 3; // 0..3 (Master, SFX, BGM, Close)
            int next = settingsIndex + dir;

            if (wrapAround)
            {
                if (next < 0) next = maxIndex;
                if (next > maxIndex) next = 0;
            }
            else
            {
                next = Mathf.Clamp(next, 0, maxIndex);
            }

            if (next != settingsIndex)
            {
                settingsIndex = next;
                HighlightSettings(settingsIndex);
            }
            return;
        }

        // MAIN MENU NAV
        if (buttons == null || buttons.Count == 0) return;

        int newIndex = mainIndex + dir;
        if (wrapAround)
        {
            if (newIndex < 0) newIndex = buttons.Count - 1;
            if (newIndex >= buttons.Count) newIndex = 0;
        }
        else
        {
            newIndex = Mathf.Clamp(newIndex, 0, buttons.Count - 1);
        }

        if (newIndex != mainIndex)
        {
            mainIndex = newIndex;
            HighlightMainButton(mainIndex);
        }
    }

    // -------------------------------------------------------
    // MAIN MENU
    // -------------------------------------------------------

    private void PressMain()
    {
        if (buttons == null || buttons.Count == 0) return;
        var btn = buttons[mainIndex];
        if (btn != null) btn.onClick.Invoke();
    }

    private void HighlightMainButton(int i)
    {
        for (int k = 0; k < buttons.Count; k++)
        {
            var btn = buttons[k];
            if (btn == null) continue;

            var colors = btn.colors;
            colors.normalColor = (k == i) ? selectedColor : normalColor;
            btn.colors = colors;
        }
    }

    public void PlayButton()
    {
        SceneManager.LoadScene("WorldOne");
    }

    public void OpenSettingsPage()
    {
        settingsPanel.SetActive(true);
        IdkPanel.SetActive(false);
        inSettings = true;
        settingsIndex = 0;
        HighlightSettings(settingsIndex);
    }

    public void CloseSettingsPage()
    {
        settingsPanel.SetActive(false);
        IdkPanel.SetActive(true);
        inSettings = false;

        // restore highlight to current main button
        HighlightMainButton(mainIndex);
    }

    // -------------------------------------------------------
    // SETTINGS MENU
    // -------------------------------------------------------

    private void HighlightSettings(int i)
    {
        Color on = selectedColor;
        Color off = normalColor;

        // Highlight Text
        if (masterText != null)
            masterText.color = (i == 0) ? on : off;

        if (sfxText != null)
            sfxText.color = (i == 1) ? on : off;

        if (bgmText != null)
            bgmText.color = (i == 2) ? on : off;

        // Highlight Slider Fill
        if (masterSlider != null && masterSlider.fillRect != null)
            masterSlider.fillRect.GetComponent<Image>().color = (i == 0) ? on : off;

        if (sfxSlider != null && sfxSlider.fillRect != null)
            sfxSlider.fillRect.GetComponent<Image>().color = (i == 1) ? on : off;

        if (bgmSlider != null && bgmSlider.fillRect != null)
            bgmSlider.fillRect.GetComponent<Image>().color = (i == 2) ? on : off;

        // Highlight CLOSE button
        if (settingsCloseButton != null)
        {
            var colors = settingsCloseButton.colors;
            colors.normalColor = (i == 3) ? on : off;
            settingsCloseButton.colors = colors;
        }
    }


    private void PressSettings()
    {
        // Only "Close" is pressable for now
        if (settingsIndex == 3 && settingsCloseButton != null)
        {
            settingsCloseButton.onClick.Invoke();
        }
    }

    // volumeDelta is e.g. +0.05f or -0.05f
    private void AdjustCurrentSetting(float volumeDelta)
    {
        switch (settingsIndex)
        {
            case 0: // Master
                AdjustSlider(masterSlider, "Master", volumeDelta);
                break;

            case 1: // SFX
                AdjustSlider(sfxSlider, "SFX", volumeDelta);
                break;

            case 2: // BGM
                AdjustSlider(bgmSlider, "BGM", volumeDelta);
                break;

            default:
                break;
        }
    }

    private void AdjustSlider(Slider slider, string mixerParam, float delta)
    {
        if (slider == null || audioMixer == null) return;

        slider.value = Mathf.Clamp01(slider.value + delta);

        // convert 0–1 slider value ? dB. Avoid log(0).
        float v = Mathf.Clamp(slider.value, 0.0001f, 1f);
        float dB = Mathf.Log10(v) * 20f;   // 0 ? -80 dB, 1 ? 0 dB-ish
        audioMixer.SetFloat(mixerParam, dB);
    }

    private void UpdateAudioMixerFromSliders()
    {
        audioMixer.SetFloat("Master", Mathf.Log10(masterSlider.value) * 20f);
        audioMixer.SetFloat("SFX", Mathf.Log10(sfxSlider.value) * 20f);
        audioMixer.SetFloat("BGM", Mathf.Log10(bgmSlider.value) * 20f);
    }

}
