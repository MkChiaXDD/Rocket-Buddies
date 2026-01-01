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
}
