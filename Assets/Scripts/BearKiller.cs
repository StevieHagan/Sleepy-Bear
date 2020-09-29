using UnityEngine;
using UnityEngine.AI;

public class BearKiller : MonoBehaviour
{//attach to hazards such as falls, fire etc to mke them kill Bear.
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Bear")
        {
            other.GetComponent<BearMover>().Die();
        }
    }
}
