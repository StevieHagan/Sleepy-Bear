using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bouncer : MonoBehaviour
{
    [SerializeField] float upwardForce = 10f;
    [SerializeField] float forwardForce = 10f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Zombie") return;

        Vector3 upwardVector = Vector3.up * upwardForce;
        Vector3 forwardVector = transform.forward * forwardForce;
        print(upwardForce);
        other.GetComponent<ZombieMover>().StartBounce(upwardVector + forwardVector);
    }

}
