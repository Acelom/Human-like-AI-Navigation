using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentKiller : MonoBehaviour
{
    public string destinationTag; 
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<NavAgent>())
        {
            other.GetComponent<NavAgent>().destination = GameObject.FindGameObjectWithTag(destinationTag);
            other.GetComponent<NavAgent>().ResetAgent();

        }
    }
}
