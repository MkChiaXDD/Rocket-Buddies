using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] private Animator anim;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private SpriteRenderer sprite;

    private void Update()
    {
        bool grounded = playerController.GetIsGrounded();
        bool goingUp = playerController.GetIsGoingUp();
        bool goingDown = playerController.GetIsGoingDown();

        // send grounded to Animator too (make a bool parameter called "IsGrounded")
        anim.SetBool("IsGrounded", grounded);

        // ? Walk ONLY when on the ground
        anim.SetBool("IsWalking", grounded && playerController.GetIsMoving());

        // ? Air logic: Jump vs Fall
        if (grounded)
        {
            anim.SetBool("IsGoingUp", false);
            anim.SetBool("IsGoingDown", false);
        }
        else
        {
            if (goingUp)
            {
                anim.SetBool("IsGoingUp", true);
                anim.SetBool("IsGoingDown", false);
            }
            else if (goingDown)
            {
                anim.SetBool("IsGoingUp", false);
                anim.SetBool("IsGoingDown", true);
            }
            else
            {
                // peak of jump: treat as falling
                anim.SetBool("IsGoingUp", false);
                anim.SetBool("IsGoingDown", true);
            }
        }

        // Facing logic
        if (playerController.GetIsAiming())
        {
            Vector2 aimDir = playerController.GetAimDirection();
            if (aimDir.x != 0)
                sprite.flipX = aimDir.x > 0f;
        }
        else
        {
            float moveDir = playerController.GetLastMoveDirection();
            sprite.flipX = moveDir > 0f;
        }
    }

    public void PlayDie()
    {
        anim.SetTrigger("Die");
    }
}
