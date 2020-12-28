using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraRotator : MonoBehaviour
{
    //[SerializeField] Transform target;
    [SerializeField] float XrotationFactor = 0.3f;
    [SerializeField] float YrotationFactor = 0.1f;
    [SerializeField] bool invertY = true;
    [SerializeField] float minVerticalAngle = 0f;
    [SerializeField] float maxVerticalAngle = 90f;
    [SerializeField] float distanceFactor = 1f;
    [SerializeField] float minDistance = 8f;
    [SerializeField] float maxDistance = 30f;

    float initialXMousePos = 0;
    float initialYMousePos = 0;
    Vector3 initialRotation;
    CinemachineComponentBase rabbitCamera;

    private void Start()
    {
        rabbitCamera = GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent(CinemachineCore.Stage.Body);
    }
    void LateUpdate()
    {
        //transform.position = target.position;
        ProcessCameraRotation();
        ProcessCameraDistance();
    }

    void ProcessCameraRotation()
    {
        if (Input.GetMouseButtonDown(1))
        {
            initialXMousePos = Input.mousePosition.x;
            initialYMousePos = Input.mousePosition.y;
            initialRotation = transform.rotation.eulerAngles;
        }

        else if (Input.GetMouseButton(1))
        {//NB X mouse coordinate translates to Y Euler rotation axis and vice versa.
            float xRotation;

            //Calculate spin-around rotation
            float yRotation = initialRotation.y + ((Input.mousePosition.x
                                                   - initialXMousePos) * XrotationFactor);

            //Calculate up and down rotation
            if (invertY)
            {
                xRotation = initialRotation.x + ((initialYMousePos
                                                   - Input.mousePosition.y) * YrotationFactor);
            }
            else
            {

                xRotation = initialRotation.x + ((Input.mousePosition.y
                                                       - initialYMousePos) * YrotationFactor);
            }
            xRotation = Mathf.Clamp(xRotation, minVerticalAngle, maxVerticalAngle);

            //Apply rotations to camera
            transform.rotation = Quaternion.Euler(xRotation,
                                                yRotation, initialRotation.z);
        }
    }

    void ProcessCameraDistance()
    {
        if(Input.GetAxis("Mouse ScrollWheel") < -0.01f)
        {//Back
            (rabbitCamera as CinemachineFramingTransposer).m_CameraDistance =
                Mathf.Clamp((rabbitCamera as CinemachineFramingTransposer).m_CameraDistance 
                            + distanceFactor, minDistance, maxDistance);
        }
        if (Input.GetAxis("Mouse ScrollWheel") > 0.01f)
        {//Forward
            (rabbitCamera as CinemachineFramingTransposer).m_CameraDistance =
                Mathf.Clamp((rabbitCamera as CinemachineFramingTransposer).m_CameraDistance
                            - distanceFactor, minDistance, maxDistance);
        }
    }
}

