using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PassiveEnemy : EnemyBase
{
    private NavMeshAgent agent;

    protected override void OnInit()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = data.speed;

        // Same 2D NavMesh setup as Bat
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    protected override void UpdateTarget()
    {
        // Passive: never detect players
        target = null;
        attackCooldown = 0f;
    }

    protected override void MoveTo(Vector3 destination)
    {
        if (agent == null) return;
        agent.SetDestination(destination);
    }

    protected override void Attack()
    {
        // Intentionally empty — passive enemy
    }
}
