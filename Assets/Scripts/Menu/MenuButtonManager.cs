using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuButtonManager : MonoBehaviour
{
    [SerializeField] private PlayerInput action;

    [Header("Buttons (top ? bottom)")]
    [SerializeField] private List<Button> buttons;

    [Header("Options")]
    [SerializeField] private bool wrapAround = true;   // allow cycling from last?first
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private Color normalColor = Color.white;

    private int index = 0;

    // Cached actions (must match names in your Input Actions)
    private InputAction nextAction;
    private InputAction prevAction;
    private InputAction submitAction; // optional

    void OnEnable()
    {
        if (action != null && action.actions != null)
        {
            nextAction = action.actions.FindAction("Next");
            prevAction = action.actions.FindAction("Previous");
            submitAction = action.actions.FindAction("Submit"); // optional

            if (nextAction != null) { nextAction.performed += _ => Move(1); nextAction.Enable(); }
            if (prevAction != null) { prevAction.performed += _ => Move(-1); prevAction.Enable(); }
            if (submitAction != null) { submitAction.performed += _ => Press(); submitAction.Enable(); }
        }

        if (buttons != null && buttons.Count > 0)
            HighlightButton(index);
    }

    void OnDisable()
    {
        if (nextAction != null) { nextAction.performed -= _ => Move(1); }
        if (prevAction != null) { prevAction.performed -= _ => Move(-1); }
        if (submitAction != null) { submitAction.performed -= _ => Press(); }
    }

    void Start()
    {
        if (buttons != null && buttons.Count > 0)
            HighlightButton(0);
    }

    private void Move(int dir)
    {
        if (buttons == null || buttons.Count == 0) return;

        int next = index + dir;
        if (wrapAround)
        {
            if (next < 0) next = buttons.Count - 1;
            if (next >= buttons.Count) next = 0;
        }
        else
        {
            next = Mathf.Clamp(next, 0, buttons.Count - 1);
        }

        if (next != index)
        {
            index = next;
            HighlightButton(index);
        }
    }

    private void Press()
    {
        if (buttons == null || buttons.Count == 0) return;
        var btn = buttons[index];
        if (btn != null) btn.onClick.Invoke();
    }

    private void HighlightButton(int i)
    {
        for (int k = 0; k < buttons.Count; k++)
        {
            var btn = buttons[k];
            if (btn == null) continue;

            var colors = btn.colors;
            colors.normalColor = (k == i) ? selectedColor : normalColor;
            btn.colors = colors; // reassign struct
        }
    }

    public void PlayButton()
    {
        SceneManager.LoadScene("WorldOne");
    }
}
