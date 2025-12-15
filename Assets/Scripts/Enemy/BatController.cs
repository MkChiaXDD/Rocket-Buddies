using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class BatController : EnemyBase
{
    private NavMeshAgent agent;
    [SerializeField] private Animator anim;

    protected override void OnInit()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = data.speed;

        // Required for 2D NavMesh
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    protected override void MoveTo(Vector3 destination)
    {
        if (agent == null) return;
        agent.SetDestination(destination);
    }

    protected override void Attack()
    {
        anim.SetTrigger("Attack");
        FindFirstObjectByType<HealthManager>()?.Damage(data.damage);
        Debug.Log($"{name} attacks target");
    }
}
