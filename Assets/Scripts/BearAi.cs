using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BearAi : MonoBehaviour
{
    [Tooltip("Vertical limit to waypoint detection, to stop Bear detecting waypoints directly above or below")]
    [SerializeField] float verticalLimit = 3f;
    [Tooltip("Sets the direct distance away from which Bear can detect waypoints")]
    [SerializeField] float onPathVisionDistance = 10f;
    [Tooltip("Sets the direct distance away from which Bear can detect waypoints")]
    [SerializeField] float offPathVisionDistance = 5f;
    [Tooltip("Sets the angle (either side of forward) within which Bear can detect the next waypoint while on a path")]
    [SerializeField] float onPathVisionAngle = 90f;
    [Tooltip("Sets the angle (either side of forward) within which Bear can detect the next waypoint while roaming loose")]
    [SerializeField] float offPathVisionAngle = 30f;

    Waypoint[] waypoints;

    private void Start()
    {
        waypoints = FindObjectsOfType<Waypoint>();
    }

    public Waypoint GetNextWaypoint(Waypoint currentDestination, float arrivalDistance, bool isOnPath)
    {
        if (currentDestination == null || currentDestination.GetManualWaypoints() == null)
        {
            return AutoDetectNextWaypoint(arrivalDistance, isOnPath);
        }
        else
        {
            return SelectFromManualWaypoints(currentDestination);
        }
    }

    public float GetTurnAngle(RaycastHit hit, bool isOnPath)
    {
        if(isOnPath)
        {
            return transform.rotation.eulerAngles.y + 180f;
        }
        else
        {
            //Get the reflection vector for Bear realtive to the obsticle normal
            Vector3 reflection = Vector3.Reflect(transform.forward, hit.normal);
            //Get angle between Bear's Forward and the reflection vector
            float angle = Vector3.SignedAngle(transform.forward, reflection, Vector3.up);
            //Add the reflection angle to Bear's forward angle
            angle += transform.rotation.eulerAngles.y;
            return angle;
        }
    }

    private Waypoint AutoDetectNextWaypoint(float arrivalDistance, bool isOnPath)
    {//finds the closest waypoint infront of Bear and returns it

        //create a HashSet of waypoints which are within Bear's detection range
        HashSet<Waypoint> detectibleWaypoints = new HashSet<Waypoint>();

        foreach (Waypoint waypoint in waypoints)
        {   //check vertical and total distnce first
            if (Mathf.Abs(transform.position.y - waypoint.transform.position.y) < verticalLimit
               && Vector3.Distance(transform.position, waypoint.transform.position) < (isOnPath ? onPathVisionDistance : offPathVisionDistance)
               && Vector3.Distance(transform.position, waypoint.transform.position) > arrivalDistance)
            {
                //create a direction vector between the Bear and the Waypoint
                Vector3 waypointDir = waypoint.transform.position - transform.position;
                //if the angle is less than required, add this Waypoint to the set
                if (Vector3.Angle(transform.forward, waypointDir) < (isOnPath ? onPathVisionAngle : offPathVisionAngle))
                {
                    detectibleWaypoints.Add(waypoint);
                }
            }
        }
        if (detectibleWaypoints.Count == 0) //there are no waypoints in range
        {
            return null;
        }
        else if (detectibleWaypoints.Count == 1) //only one waypoint is in range, return it
        {
            foreach (Waypoint waypoint in detectibleWaypoints)
            {
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
            return closestWaypoint;

        }
    }

    private Waypoint SelectFromManualWaypoints(Waypoint currentDestination)
    {//finds which of the manually set waypoints is closest to Bear's forward

        Waypoint[] manualWaypoints = currentDestination.GetManualWaypoints();

        if (manualWaypoints.Length == 0)
        {
            return null;
        }
        else if (manualWaypoints.Length == 1)
        {
            return manualWaypoints[0];
        }
        else
        {
            Waypoint forwardestWaypoint = null;
            float narrowestAngle = 360f;
            bool honeyPotWaypointsOnly = false;

            foreach(Waypoint waypoint in manualWaypoints)
            {
                if(waypoint.HasHoneyPot())
                {
                    honeyPotWaypointsOnly = true;
                }
            }

            foreach (Waypoint waypoint in manualWaypoints)
            {
                Vector3 targetDir = waypoint.transform.position - transform.position;
                float targetAngle = Vector3.Angle(targetDir, transform.forward);

                if (honeyPotWaypointsOnly && !waypoint.HasHoneyPot()) continue;

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
