using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BearMover : MonoBehaviour
{
    [Tooltip("Sets the distance under which Bear will stop walking towards the current waypoint and look for the next one")]
    [SerializeField] float arrivalDistance = 0.5f;
    [Tooltip("Sets the direct distance away from which Bear can detect waypoints")]
    [SerializeField] float visionDistance = 10f;
    [Tooltip("Vertical limit to waypoint detection, to stop Bear detecting waypoints directly above or below")]
    [SerializeField] float verticalLimit = 3f;
    [Tooltip("Sets the angle (either side of forward) within which Bear can detect the next waypoint")]
    [SerializeField] float visionAngle = 90f;
    [Tooltip("READ ONLY - Indicates if Bear is currently on a path or roaming loose.")]
    [SerializeField] bool isOnPath = true;

    Waypoint[] waypoints;
    Waypoint currentDestination;
    NavMeshAgent navAgent;
    CapsuleCollider mainCollider;

    float timeOfLastWaypointCheck = 0f;

    void Start()
    {
        mainCollider = GetComponent<CapsuleCollider>();
        navAgent = GetComponent<NavMeshAgent>();
        waypoints = FindObjectsOfType<Waypoint>();

        currentDestination = GetNextWaypoint();
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
            currentDestination = GetNextWaypoint();
        }
    }

    private void ProcessOffPathMovement()
    {
        isOnPath = false;
        navAgent.SetDestination(transform.position + transform.forward * 2);

        if (Time.time - timeOfLastWaypointCheck > 1) //if more than a second has passed since Bear last looked for a waypoint
        {
            currentDestination = GetNextWaypoint();
            timeOfLastWaypointCheck = Time.time;
        }
    }

    private Waypoint GetNextWaypoint()
    {
        if(currentDestination == null || currentDestination.GetManualWaypoints() == null)
        {
            return AutoDetectNextWaypoint();
        }
        else
        {
            return SelectFromManualWaypoints();
        }
    }

    private Waypoint AutoDetectNextWaypoint()
    {//finds the closest waypoint infront of Bear and returns it


        //create a HashSet of waypoints which are within Bear's detection range
        HashSet<Waypoint> detectibleWaypoints = new HashSet<Waypoint>();

        foreach (Waypoint waypoint in waypoints)
        {   //check vertical and total distnce first
            if (Mathf.Abs(transform.position.y - waypoint.transform.position.y) < verticalLimit
               && Vector3.Distance(transform.position, waypoint.transform.position) < visionDistance
               && Vector3.Distance(transform.position, waypoint.transform.position) > arrivalDistance)
            {
                //create a direction vector between the Bear and the Waypoint
                Vector3 waypointDir = waypoint.transform.position - transform.position;
                //if the angle is less than required, add this Waypoint to the set
                if (Vector3.Angle(transform.forward, waypointDir) < visionAngle)
                {
                    detectibleWaypoints.Add(waypoint);
                }
            }
        }
        if (detectibleWaypoints.Count == 0) //there are no waypoints in range
        {
            Debug.Log("There are no waypoints within detection range");
            return null;
        }
        else if (detectibleWaypoints.Count == 1) //only one waypoint is in range, return it
        {
            foreach (Waypoint waypoint in detectibleWaypoints)
            {
                Debug.Log("There is one waypoint in range at position " + waypoint.transform.position);
                return waypoint;
            }
            return null;
        }
        else //multiple waypoints, find which one is closest to Bear.
        {
            Waypoint closestWaypoint = null;
            float closestDistance = 1000f;

            foreach (Waypoint waypoint in detectibleWaypoints)
            {
                float distance = Vector3.Distance(transform.position, waypoint.transform.position);
                if (distance < closestDistance)
                {
                    closestWaypoint = waypoint;
                    closestDistance = distance;
                }
            }
            Debug.Log("There are " + detectibleWaypoints.Count + " waypoints in range.");
            return closestWaypoint;

        }
    }

    private Waypoint SelectFromManualWaypoints()
    {//finds which of the manually set waypoints is closest to Bear's forward

        Waypoint[] manualWaypoints = currentDestination.GetManualWaypoints();

        if(manualWaypoints.Length == 0)
        {
            return null;
        }
        else if(manualWaypoints.Length == 1)
        {
            return manualWaypoints[0];
        }
        else
        {
            Waypoint forwardestWaypoint = null;
            float narrowestAngle = 360f;

            foreach (Waypoint waypoint in manualWaypoints)
            {
                Vector3 targetDir = waypoint.transform.position - transform.position;
                float targetAngle = Vector3.Angle(targetDir, transform.forward);
                if (targetAngle < narrowestAngle)
                {
                    forwardestWaypoint = waypoint;
                    narrowestAngle = targetAngle;
                }
            }
            return forwardestWaypoint;
        }
    }
}
