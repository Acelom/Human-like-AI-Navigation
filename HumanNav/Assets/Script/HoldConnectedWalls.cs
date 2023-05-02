using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldConnectedWalls : MonoBehaviour
{

    public List<GameObject> connectedWalls;

    private void Awake()
    {
        connectedWalls = new List<GameObject>(); 
    }

    public void AddConnectedWall(GameObject newConnection)
    {
        connectedWalls.Add(newConnection); 
    }

}
