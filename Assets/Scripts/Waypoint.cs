using System.ComponentModel;
using UnityEngine;

public class Waypoint : MonoBehaviour
{
    [Tooltip("Add waypoints here to override the automatic detection system. " +
              "Bear will travel to whichever of these are closest to his Forward")]
    [SerializeField] Waypoint[] WaypointsOverride;


    public Waypoint[] GetManualWaypoints()
    {
        if(WaypointsOverride.Length > 0)
        {
            return WaypointsOverride;
        }
        else
        {
            return null;
        }
    }
}
