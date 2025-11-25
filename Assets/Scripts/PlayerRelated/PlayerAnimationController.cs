using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] private Animator anim;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private SpriteRenderer sprite;

    private void Update()
    {
        // Walking animation
        anim.SetBool("IsWalking", playerController.GetIsMoving());

        // Facing logic
        if (playerController.GetIsAiming())
        {
            // Face aim direction
            Vector2 aimDir = playerController.GetAimDirection();
            if (aimDir.x != 0)
                sprite.flipX = aimDir.x > 0f;
        }
        else
        {
            // Face movement direction
            float moveDir = playerController.GetLastMoveDirection();
            sprite.flipX = moveDir > 0f;
        }
    }
}
