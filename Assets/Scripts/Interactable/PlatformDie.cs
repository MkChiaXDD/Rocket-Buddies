using UnityEngine;

public class PlatformDie : MonoBehaviour
{
    private HealthManager hpMgr;
    [SerializeField] private BoxCollider2D death;
    [SerializeField] private MovingPlatform platform;

    void Start()
    {
        hpMgr = FindFirstObjectByType<HealthManager>();

        if (platform == null)
        {
            gameObject.GetComponentInParent<MovingPlatform>();
        }

        if (death == null)
        {
            death = GetComponent<BoxCollider2D>();
        }
    }

    private void Update()
    {
        if (!platform.GetIsGoingUp())
        {
            death.enabled = true;
        }
        else
        {
            death.enabled = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name == "Player1" || collision.gameObject.name == "Player2")
        {
            hpMgr.Damage(10);
        }
    }
}
