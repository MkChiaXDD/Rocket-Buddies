using TMPro;
using UnityEngine;

public class DeathCounter : MonoBehaviour
{
    [SerializeField] private TMP_Text blueDeathCounterText;
    [SerializeField] private TMP_Text redDeathCounterText;

    private int blueDeathCounter;
    private int redDeathCounter;

    public void IncreaseDeath(string playerName)
    {
        if (playerName == "Player1")
        {
            blueDeathCounter++;
        }
        else
        {
            redDeathCounter++;
        }

        UpdateText();
    }

    private void UpdateText()
    {
        blueDeathCounterText.text = blueDeathCounter.ToString();
        redDeathCounterText.text = redDeathCounter.ToString();
    }
}
