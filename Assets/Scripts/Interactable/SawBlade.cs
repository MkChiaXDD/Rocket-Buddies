using System.Collections.Generic;
using UnityEngine;

public class SawBlade : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3.0f;
    [SerializeField] private List<Transform> waypoints;
    private HealthManager hpMgr;

    private int currentWaypoint = 0;

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

        // check if reached waypoint
        if (Vector3.Distance(transform.position, target.position) < 0.05f)
        {
            currentWaypoint++;

            // loop back to first (0)
            if (currentWaypoint >= waypoints.Count)
            {
                currentWaypoint = 0;
            }
        }

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
