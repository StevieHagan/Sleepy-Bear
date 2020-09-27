using UnityEngine;
using UnityEngine.AI;

public class BearKiller : MonoBehaviour
{//attach to hazards such as falls, fire etc to mke them kill Bear.
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Bear")
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            NavMeshAgent nav = other.GetComponent<NavMeshAgent>();

            float speed = nav.speed;
            nav.enabled = false;
            rb.isKinematic = false;
            rb.velocity = other.transform.forward * speed;

            other.GetComponent<BearMover>().Die();
        }
    }
}
