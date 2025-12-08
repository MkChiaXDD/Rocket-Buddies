using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    [SerializeField] private Portal linkedPortal;

    private bool player1Reached = false;
    private bool player2Reached = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name == "Player1")
        {
            player1Reached = true;
        }
        else if (collision.gameObject.name == "Player2")
        {
            player2Reached = true;
        }
        else
        {
            return; // not a player
        }

        if (player1Reached && player2Reached)
        {
           
        }
    }
}
