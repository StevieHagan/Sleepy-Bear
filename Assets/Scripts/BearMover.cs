using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BearMover : MonoBehaviour
{
    [Tooltip("Sets the distance under which Bear will stop walking towards the current waypoint and look for the next one")]
    [SerializeField] float arrivalDistance = 0.5f;
    [Tooltip("READ ONLY - Indicates if Bear is currently on a path or roaming loose.")]
    [SerializeField] bool isOnPath = false;

    BearAi ai;
    Waypoint currentDestination;
    NavMeshAgent navAgent;
    CapsuleCollider mainCollider;

    float timeOfLastWaypointCheck = 0f;

    void Start()
    {
        ai = GetComponent<BearAi>();
        mainCollider = GetComponent<CapsuleCollider>();
        navAgent = GetComponent<NavMeshAgent>();

        currentDestination = ai.GetNextWaypoint(currentDestination, arrivalDistance, isOnPath);
    }
    void Update()
    {
        if (currentDestination != null)
        {
            ProcessPathMovement();
        }
        else
        {
            ProcessOffPathMovement();
        }
    }

    private void ProcessPathMovement()
    {
        isOnPath = true;

        navAgent.SetDestination(currentDestination.transform.position);

        if (Vector3.Distance(transform.position, currentDestination.transform.position) <= arrivalDistance)
        {
            print("close");
            currentDestination = ai.GetNextWaypoint(currentDestination, arrivalDistance, isOnPath);
        }
    }

    private void ProcessOffPathMovement()
    {
        isOnPath = false;
        navAgent.SetDestination(transform.position + transform.forward * 2);

        if (Time.time - timeOfLastWaypointCheck > 1) //if more than a second has passed since Bear last looked for a waypoint
        {
            currentDestination = ai.GetNextWaypoint(currentDestination, arrivalDistance, isOnPath);
            timeOfLastWaypointCheck = Time.time;
        }
    }

        }
    
