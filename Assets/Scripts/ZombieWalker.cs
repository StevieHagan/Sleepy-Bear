using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using TreeEditor;
using UnityEngine;

public class ZombieWalker : MonoBehaviour
{

    [SerializeField] float defaultSpeed = 1f;
    [SerializeField] float acceleration = 10f;
    [Tooltip("Distance from destination to end guided movement")]
    [SerializeField] float cancelDestinationDistance = 0.5f;
    [Tooltip("Time taken to turn, in seconds")]
    [SerializeField] float turnSpeed = 2f;

    Vector3 currentDestination;
    Rigidbody rb;
    float targetSpeed;
    float currentSpeed = 0f;
    float startAngle = Mathf.Infinity;
    float targetAngle;
    float timer;
    bool hasValidDestination;
    bool aboutFace = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        targetAngle = transform.rotation.eulerAngles.y;
        targetSpeed = defaultSpeed;
    }

    void FixedUpdate()
    {
        SetTargetAngle();
        AdjustVelocity();
        AdjustRotation();
    }

    public bool IsTurning()
    {
        return (startAngle != Mathf.Infinity);
    }

    public void SetDestination(Vector3 destination, float speedFactor = 1f)
    {
        
        if(destination != currentDestination)
        {
            currentDestination = destination;
            AppendTurnInProgress();
            hasValidDestination = true;
        }
        else if(Vector3.Distance(transform.position, destination) < cancelDestinationDistance)
        {
            hasValidDestination = false;
        }
        targetSpeed = defaultSpeed * speedFactor;
    }

    public void SetTargetAngle(float angle = Mathf.Infinity)
    {

        if (angle != Mathf.Infinity)//ie an angle argument has been passed
        {
            hasValidDestination = false;
            targetAngle = angle;
            AppendTurnInProgress();
        }
        else if (hasValidDestination && !aboutFace)
        {
            targetAngle = NormaliseAngle(Vector3.SignedAngle(Vector3.forward, currentDestination - transform.position, Vector3.up));
        }
    }

    private void AdjustRotation()
    {
        if (Mathf.Abs(Mathf.DeltaAngle(transform.rotation.eulerAngles.y, targetAngle)) > 0.1f)
        {
            if (startAngle == Mathf.Infinity)//Set conditions to start turning
            {
                timer = Time.time;
                startAngle = transform.rotation.eulerAngles.y;
                //if the turn is close to 180deg then set the aboutFace variable to disable targetAngle updates during the turn.
                if(Mathf.Abs(Mathf.DeltaAngle(startAngle, targetAngle)) > 170f)
                {
                    aboutFace = true;
                }
            }
            float interimAngle = startAngle + (Mathf.DeltaAngle(startAngle, targetAngle) * 0.5f *
                                 (Mathf.Sin(((((Time.time - timer) / turnSpeed) - 0.5f) * 180) * Mathf.Deg2Rad) + 1));
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, interimAngle, transform.rotation.eulerAngles.z);
        }
        else if(aboutFace && Time.time - timer >= turnSpeed)
        {//aboutFace turn has completed, start a new turn towards the current target
            aboutFace = false;
            startAngle = Mathf.Infinity;
        }
        else //Set conditions to end the turn
        {
            startAngle = Mathf.Infinity;
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, targetAngle, transform.rotation.eulerAngles.z);
        }
    }

    private void AppendTurnInProgress()
    {//if the destination or forward angle are changed while a turn is already in progress
     //this will adjust timer and startAngle to ensure smooth continuation of movement
        if(startAngle != Mathf.Infinity)
        {
                float timerOffset = Mathf.Abs((turnSpeed / 2) - (Time.time - timer));
                timer = Time.time - ((turnSpeed / 2) - timerOffset);
                startAngle = transform.rotation.eulerAngles.y;
        }//TODO Change this so that it calculates the rate of change in the old movement and then finds where on the timeline the new delta will match
    }

    private void AdjustVelocity()
    {
        if (Mathf.Abs(targetSpeed - currentSpeed) > 0.01f)
        {
            currentSpeed += acceleration * Time.fixedDeltaTime * (targetSpeed - currentSpeed);
        }
        else
        {
            currentSpeed = targetSpeed;
        }
        Vector3 newVelocity = new Vector3(transform.forward.x * currentSpeed,
                                          rb.velocity.y,
                                          transform.forward.z * currentSpeed);
        rb.velocity = newVelocity;
    }

    private void CheckForArrival()
    {
        if(Vector3.Distance(currentDestination, transform.position) < cancelDestinationDistance)
        {
            hasValidDestination = false;
        }
    }


    private float NormaliseAngle(float angle)
    {
        while(angle > 180.0f)
        {
            angle -= 360f;
        }
        while(angle < -180f)
        {
            angle += 360f;
        }
        return angle;
    }
}
