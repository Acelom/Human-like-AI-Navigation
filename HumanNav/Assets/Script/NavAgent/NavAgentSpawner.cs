using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavAgentSpawner : MonoBehaviour
{
    private List<GameObject> agents;
    private GameObject agent;
    [SerializeField] private int agentCount; 

    private void Awake()
    {
        agent = Resources.Load<GameObject>("Prefabs/AIAgents/NavAgent");
        agents = new List<GameObject>(); 
    }

    private void Update()
    {
        if (agents.Count < agentCount)
        {
            agents.Add(Instantiate(agent, transform.position, transform.rotation)); 
        }
    }


}
