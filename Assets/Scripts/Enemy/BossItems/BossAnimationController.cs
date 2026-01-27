using UnityEngine;

public class BossAnimationController : MonoBehaviour
{
    [SerializeField] private Animator anim;
    [SerializeField] private BossController boss;
    [SerializeField] private SpriteRenderer sprite;

    private void Awake()
    {
        if (boss == null)
            boss = GetComponentInParent<BossController>();
    }

    public void SetFacing(bool faceLeft)
    {
        sprite.flipX = faceLeft;
    }

    public void PlayShootAnim()
    {
        anim.SetTrigger("Shoot");
    }

    public void StartAbilityAnim()
    {
        anim.SetTrigger("AbilityStart");
    }

    public void EndAbilityAnim()
    {
        anim.SetTrigger("AbilityEnd");
    }

    public void PlayIdleAnim()
    {
        anim.SetTrigger("Idle");
    }
}
