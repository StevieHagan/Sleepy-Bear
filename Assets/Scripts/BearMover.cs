using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BearMover : MonoBehaviour
{
    [Tooltip("Sets the distance under which Bear will stop walking towards the current waypoint and look for the next one")]
    [SerializeField] float arrivalDistance = 0.5f;
    [Tooltip("Time taken to turn after meeting an obstacle, in seconds")]
    [SerializeField] float turnSpeed = 2f;
    [Tooltip("READ ONLY - Indicates if Bear is currently on a path or roaming loose.")]
    [SerializeField] bool isOnPath = false;
    [SerializeField] States state = States.walking;

    BearAi ai;
    Waypoint currentDestination;
    NavMeshAgent navAgent;
    Rigidbody rb;
    CapsuleCollider mainCollider;

    float timer = 0f;
    int deflectorLayerMask;
    float startAngle = 0; //for turning when encountering an obsticle
    float targetAngle = 0; 

    void Start()
    {
        ai = GetComponent<BearAi>();
        mainCollider = GetComponent<CapsuleCollider>();
        navAgent = GetComponent<NavMeshAgent>();
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
                ProcessTurn();
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
            rb.isKinematic = true;
            navAgent.enabled = true;
        }

        state = newState;
    }

    public void Die()
    {
        ChangeState(States.dead);

        float speed = navAgent.speed;
        rb.isKinematic = false;
        navAgent.enabled = false;
        rb.velocity = transform.forward * speed * 2;

    }

    private void ProcessPathMovement()
    {
        isOnPath = true;

        navAgent.SetDestination(currentDestination.transform.position);

        if (Vector3.Distance(transform.position, currentDestination.transform.position) <= arrivalDistance)
        {
            currentDestination = ai.GetNextWaypoint(currentDestination, arrivalDistance, isOnPath);
        }
    }

    private void ProcessOffPathMovement()
    {
        isOnPath = false;
        navAgent.SetDestination(transform.position + transform.forward * 2);

        if (Time.time - timer > 1) //if more than a second has passed since Bear last looked for a waypoint
        {
            currentDestination = ai.GetNextWaypoint(currentDestination, arrivalDistance, isOnPath);
            timer = Time.time;
        }
    }

    private void CheckForDeflector()
    {
        RaycastHit hit;

        Debug.DrawRay(transform.position + Vector3.up, transform.forward * 3);
        if (Physics.Raycast(transform.position + Vector3.up, transform.forward, out hit, 3.0f, deflectorLayerMask))
        {
            print(hit.normal);
            StartTurn(ai.GetTurnAngle(hit, isOnPath));
        }
    }

    private void StartTurn(float targetAngle)
    { //Turning when an obsticle is encountered.

        ChangeState(States.turning);
        navAgent.enabled = false;
        startAngle = transform.rotation.eulerAngles.y;
        this.targetAngle = targetAngle;
        timer = Time.time;

        ProcessTurn();
    }

    private void ProcessTurn()
    {
        if (Mathf.Abs(Mathf.DeltaAngle(transform.rotation.eulerAngles.y, targetAngle)) > 0.1)
        {

            float interimAngle = startAngle + ((targetAngle - startAngle) * 0.5f *
                                 (Mathf.Sin(((((Time.time - timer) / turnSpeed) -0.5f) * 180) * Mathf.Deg2Rad) + 1));

            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, interimAngle, transform.rotation.eulerAngles.z);
        }
        else
        {
            ChangeState(States.walking);
        }
    }
}
    
