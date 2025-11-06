using UnityEngine;

public class StepButton : MonoBehaviour
{
    [SerializeField] private GameObject unPressedButton;
    [SerializeField] private GameObject pressedButton;

    private void Start()
    {
        if (unPressedButton) unPressedButton.SetActive(true);
        if (pressedButton) pressedButton.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            SetButton(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            SetButton(false);
        }
    }

    private void SetButton(bool pressed)
    {
        if (unPressedButton) unPressedButton.SetActive(!pressed);
        if (pressedButton) pressedButton.SetActive(pressed);
    }
}
