﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BearMover : MonoBehaviour
{
    [Tooltip("Sets the distance under which Bear will stop walking towards the current waypoint and look for the next one")]
    [SerializeField] float arrivalDistance = 0.5f;
    [Tooltip("Time taken to turn after meeting an obstacle, in seconds")]
    [SerializeField] float turnSpeed = 2f;
    [Tooltip("Minimum amount of time a bounce can last for. Prevents going immediately back to walk state due to low velocity")]
    [SerializeField] float minBounceTime = 0.5f;
    [Tooltip("READ ONLY - Indicates if Bear is currently on a path or roaming loose.")]
    [SerializeField] bool isOnPath = false;
    [SerializeField] States state = States.walking;

    BearAi ai;
    BearWalker walker;
    Waypoint currentDestination;
    Rigidbody rb;
    CapsuleCollider mainCollider;
    Vector3 forceToApplyNextFixedUpdate;

    float timer = 0f;
    int deflectorLayerMask;
    float startAngle = 0; //for turning when encountering an obsticle
    float targetAngle = 0;
    bool applyForceNextFixedUpdate = false;

    void Start()
    {
        ai = GetComponent<BearAi>();
        walker = GetComponent<BearWalker>();
        mainCollider = GetComponent<CapsuleCollider>();
        rb = GetComponent<Rigidbody>();
        deflectorLayerMask = 1 << 8;

        currentDestination = ai.GetNextWaypoint(currentDestination, arrivalDistance, isOnPath);
    }
    void Update()
    {
        switch(state)
        {
            case States.walking:
                if (currentDestination != null)
                {
                    ProcessPathMovement();
                }
                else
                {
                    ProcessOffPathMovement();
                }                
                CheckForDeflector();
                break;
            case States.turning:
                if(!walker.isTurning())
                {
                    ChangeState(States.walking);
                }
                break;
            case States.bouncing:
                ProcessBounce();
                break;
            case States.dead:
                break;
        }
    }

    private void ChangeState(States newState)
    {
        if (newState == States.walking)
        {
            currentDestination = null;
            walker.enabled = true;
        }

        state = newState;
    }

    public void Die()
    {
        ChangeState(States.dead);
        walker.enabled = false;
        rb.freezeRotation = false;
    }

   public void StartBounce(Vector3 bounceForce)
    {
        //prevent a second bounce being called if one has just started
        if (state == States.bouncing && Time.time - timer < minBounceTime) return;

        state = States.bouncing;
        walker.enabled = false;
        timer = Time.time;
        rb.AddForce(bounceForce, ForceMode.Impulse);
    }

    private void ProcessPathMovement()
    {
        isOnPath = true;

        walker.SetDestination(currentDestination.transform.position, 1);

        if (Vector3.Distance(transform.position, currentDestination.transform.position) <= arrivalDistance)
        {
            currentDestination = ai.GetNextWaypoint(currentDestination, arrivalDistance, isOnPath);
        }
    }

    private void ProcessOffPathMovement()
    {
        isOnPath = false;
        //walker.SetDestination(transform.position + transform.forward * 2);

        if (Time.time - timer > 1) //if more than a second has passed since Bear last looked for a waypoint
        {
            currentDestination = ai.GetNextWaypoint(currentDestination, arrivalDistance, isOnPath);
            timer = Time.time;
        }
    }

    private void CheckForDeflector()
    {
        RaycastHit hit;

        //Debug.DrawRay(transform.position + Vector3.up, transform.forward * 3);
        if (Physics.Raycast(transform.position + Vector3.up, transform.forward, out hit, 3.0f, deflectorLayerMask))
        {
            print(hit.normal);
            Deflect(ai.GetTurnAngle(hit, isOnPath));
        }
    }

    private void Deflect(float targetAngle)
    { //Turning when an obsticle is encountered.

        ChangeState(States.turning);
        walker.SetTargetAngle(targetAngle);
    }

    private void ProcessBounce()
    {
        //If minimum bounce time has not passed or Bear has not yet come to rest, do nothing
        if (Time.time - timer < minBounceTime || Vector3.SqrMagnitude(rb.velocity) > 0.1f) return;

        ChangeState(States.walking);
    }
}
    
