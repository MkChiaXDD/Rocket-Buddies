using UnityEngine;

public class Portal : MonoBehaviour
{
    [SerializeField] private Portal linkedPortal;
    [SerializeField] private bool IsExitPortal = false;
    [SerializeField] private string popupText; 

    private bool player1Reached = false;
    private bool player2Reached = false;

    private GameObject player1;
    private GameObject player2;

    [SerializeField] private BossController boss;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (linkedPortal == null) return;
        if (IsExitPortal) return;

        if (collision.gameObject.name == "Player1")
        {
            player1Reached = true;
            player1 = collision.gameObject;
        }
        else if (collision.gameObject.name == "Player2")
        {
            player2Reached = true;
            player2 = collision.gameObject;
        }
        else
        {
            return; // not a player
        }

        if (player1Reached && player2Reached)
        {
            if (player1 && player2 && linkedPortal)
            {
                AudioManager.Instance.PlaySFX("Teleport", 0.1f);
                if (popupText != null)
                {
                    FloatingText.Instance.Show(popupText);
                }
                player1.transform.position = linkedPortal.transform.position;
                player2.transform.position = linkedPortal.transform.position;

                if (boss != null)
                {
                    boss.StartBossFight();
                    FindFirstObjectByType<CameraController>()?.SetSharedWithTarget(boss.transform);
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (linkedPortal == null) return;
        if (IsExitPortal) return;

        if (collision.gameObject.name == "Player1")
        {
            player1Reached = false;
            player1 = null;
        }
        else if (collision.gameObject.name == "Player2")
        {
            player2Reached = false;
            player2 = null;
        }
        else
        {
            return;
        }
    }
}
