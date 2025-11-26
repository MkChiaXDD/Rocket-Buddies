using System.Collections.Generic;
using UnityEngine;

public class SawBlade : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3.0f;
    [SerializeField] private List<Transform> waypoints;
    private HealthManager hpMgr;

    private int currentWaypoint = 0;
    private int direction = 1; // +1 = forward, -1 = backwards

    [SerializeField] private float rotateSpeed = 10f;

    private void Start()
    {
        hpMgr = FindFirstObjectByType<HealthManager>();
    }

    void Update()
    {
        if (waypoints.Count == 0) return;

        // move towards target waypoint
        Transform target = waypoints[currentWaypoint];
        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            moveSpeed * Time.deltaTime
        );

        // reached waypoint?
        if (Vector3.Distance(transform.position, target.position) < 0.05f)
        {
            // Move forward or backward depending on direction
            currentWaypoint += direction;

            // Reverse direction at either end
            if (currentWaypoint >= waypoints.Count)
            {
                currentWaypoint = waypoints.Count - 2; // bounce back
                direction = -1;
            }
            else if (currentWaypoint < 0)
            {
                currentWaypoint = 1; // bounce forward
                direction = 1;
            }
        }

        // spin
        transform.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            hpMgr.Damage(10);
        }
    }
}
